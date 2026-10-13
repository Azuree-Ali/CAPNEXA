using CAPNEXAAPI.DataAccess;
using CAPNEXAAPI.Models;
using CAPNEXAAPI.Services;
using CAPNEXAAPI.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace CAPNEXAAPI;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>()
            ?? throw new InvalidOperationException("The 'Jwt' configuration section is missing.");

        if (string.IsNullOrWhiteSpace(jwtSettings.Key))
        {
            throw new InvalidOperationException(
                "The JWT signing key is not configured. Set 'Jwt:Key' (at least 32 characters) using user-secrets or the JWT__KEY environment variable.");
        }

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = true;
        })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        var authBuilder = services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.Name
                };
            })
            .AddCookie(IdentityConstants.ApplicationScheme, options =>
            {
                options.Cookie.Name = IdentityConstants.ApplicationScheme;
                options.LoginPath = "/";
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
            })
            .AddCookie(IdentityConstants.ExternalScheme, options =>
            {
                options.Cookie.Name = IdentityConstants.ExternalScheme;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
            });

        var googleClientId = GetSecret(configuration, "Authentication:Google:ClientId", "Auth:Google:ClientId");
        var googleClientSecret = GetSecret(configuration, "Authentication:Google:ClientSecret", "Auth:Google:ClientSecret");

        if (IsConfigured(googleClientId, googleClientSecret))
        {
            authBuilder.AddOpenIdConnect("Google", options =>
            {
                options.Authority = "https://accounts.google.com";
                options.ClientId = googleClientId!;
                options.ClientSecret = googleClientSecret!;
                options.CallbackPath = "/api/auth/google/callback";
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.ResponseType = "code";
                options.ResponseMode = "query";
                options.UsePkce = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.SaveTokens = false;
                options.CorrelationCookie = new CookieBuilder
                {
                    Name = ".AspNetCore.Correlation.Google",
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax,
                    SecurePolicy = CookieSecurePolicy.SameAsRequest,
                    IsEssential = true,
                    Expiration = TimeSpan.FromMinutes(15)
                };
                options.NonceCookie = new CookieBuilder
                {
                    Name = ".AspNetCore.OpenIdConnect.Nonce.Google",
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax,
                    SecurePolicy = CookieSecurePolicy.SameAsRequest,
                    IsEssential = true,
                    Expiration = TimeSpan.FromMinutes(15)
                };
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");

                options.Events = new OpenIdConnectEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var externalAuth = context.HttpContext.RequestServices.GetRequiredService<IExternalAuthService>();
                        var principal = context.Principal;

                        var providerKey = principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                                          ?? principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);
                        var email = principal?.FindFirstValue(ClaimTypes.Email)
                                    ?? principal?.FindFirstValue("email");
                        var emailVerified = bool.TryParse(principal?.FindFirstValue("email_verified"), out var verified) && verified;
                        var firstName = principal?.FindFirstValue(ClaimTypes.GivenName)
                                       ?? principal?.FindFirstValue("given_name");
                        var lastName = principal?.FindFirstValue(ClaimTypes.Surname)
                                      ?? principal?.FindFirstValue("family_name");
                        var displayName = principal?.FindFirstValue("name")
                                          ?? principal?.FindFirstValue(ClaimTypes.Name);

                        var result = await externalAuth.ProcessAsync(
                            "Google", providerKey ?? string.Empty, email, emailVerified,
                            firstName, lastName, displayName);

                        var properties = context.Properties ??= new AuthenticationProperties();
                        properties.RedirectUri = BuildSpaRedirect(context.HttpContext, result);
                    }
                };
            });

        }

        var githubClientId = GetSecret(configuration, "Authentication:GitHub:ClientId", "Auth:GitHub:ClientId");
        var githubClientSecret = GetSecret(configuration, "Authentication:GitHub:ClientSecret", "Auth:GitHub:ClientSecret");
        var githubCallbackPath = configuration["Auth:GitHub:CallbackPath"]
            ?? configuration["Authentication:GitHub:CallbackPath"]
            ?? "/api/auth/github/callback";

        if (IsConfigured(githubClientId, githubClientSecret))
        {
            authBuilder.AddOAuth("GitHub", options =>
            {
                options.ClientId = githubClientId!;
                options.ClientSecret = githubClientSecret!;
                options.CallbackPath = githubCallbackPath;
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
                options.TokenEndpoint = "https://github.com/login/oauth/access_token";
                options.UserInformationEndpoint = "https://api.github.com/user";
                options.Scope.Add("read:user");
                options.Scope.Add("user:email");
                options.UsePkce = true;
                options.SaveTokens = false;
                options.CorrelationCookie = new CookieBuilder
                {
                    Name = ".AspNetCore.Correlation.GitHub",
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax,
                    SecurePolicy = CookieSecurePolicy.SameAsRequest,
                    IsEssential = true,
                    Expiration = TimeSpan.FromMinutes(15)
                };

                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = async context =>
                    {
                        var profile = context.AccessToken is { Length: > 0 } accessToken
                            ? await FetchGitHubProfileAsync(accessToken, context.Options.UserInformationEndpoint)
                            : null;

                        if (profile is null)
                        {
                            context.Properties ??= new AuthenticationProperties();
                            context.Properties.Items["github_profile_failed"] = "1";
                            return;
                        }

                        var identity = context.Principal?.Identity as ClaimsIdentity;
                        if (identity is null)
                        {
                            identity = new ClaimsIdentity("GitHub");
                            context.Principal = new ClaimsPrincipal(identity);
                        }

                        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, profile.ProviderKey));

                        if (!string.IsNullOrWhiteSpace(profile.DisplayName))
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Name, profile.DisplayName));
                            identity.AddClaim(new Claim(ClaimTypes.GivenName, profile.DisplayName));
                        }

                        if (profile.VerifiedEmail is not null)
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Email, profile.VerifiedEmail));
                        }
                        else if (profile.AnyEmail is not null)
                        {
                            context.Properties ??= new AuthenticationProperties();
                            context.Properties.Items["github_email_present_unverified"] = profile.AnyEmail;
                        }
                    },
                    OnTicketReceived = async context =>
                    {
                        if (context.Properties is not null &&
                            context.Properties.Items.ContainsKey("github_profile_failed"))
                        {
                            context.ReturnUri = BuildFrontendErrorUrl(context.HttpContext, "github_profile_failed");
                            return;
                        }

                        var principal = context.Principal;

                        var providerKey = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                        var verifiedEmail = principal?.FindFirstValue(ClaimTypes.Email);
                        var displayName = principal?.FindFirstValue(ClaimTypes.GivenName)
                                          ?? principal?.FindFirstValue(ClaimTypes.Name);

                        string? unverifiedEmail = null;
                        if (context.Properties is not null &&
                            context.Properties.Items.TryGetValue("github_email_present_unverified", out var unverifiedRaw) &&
                            !string.IsNullOrWhiteSpace(unverifiedRaw))
                        {
                            unverifiedEmail = unverifiedRaw;
                        }

                        var email = verifiedEmail ?? unverifiedEmail;

                        var externalAuth = context.HttpContext.RequestServices.GetRequiredService<IExternalAuthService>();
                        var result = await externalAuth.ProcessAsync(
                            "GitHub", providerKey ?? string.Empty, email,
                            verifiedEmail is not null, displayName, null, displayName);

                        context.ReturnUri = BuildSpaRedirect(context.HttpContext, result);
                    },
                    OnRemoteFailure = context =>
                    {
                        RedirectFrontendWithError(context.HttpContext, "oauth_failed");
                        context.HandleResponse();
                        return Task.CompletedTask;
                    }
                };
            });
        }

        services.AddAuthorization();

        services.AddCors(options =>
        {
            var frontendOrigins = (configuration["AppUrls:FrontendBaseUrl"] ?? string.Empty)
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            options.AddPolicy("Frontend", policy =>
            {
                policy.WithOrigins(frontendOrigins.Length > 0 ? frontendOrigins : ["http://localhost:5173"])
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddFixedWindowLimiter("auth", limiter =>
            {
                limiter.Window = TimeSpan.FromMinutes(1);
                limiter.PermitLimit = 10;
                limiter.QueueLimit = 0;
            });
        });

        services.AddMemoryCache();

        services.AddSingleton<ExchangeCodeService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<RefreshTokenService>();
        services.AddScoped<IExternalAuthService, ExternalAuthService>();

        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.AddTransient<IEmailSender, EmailSender>();

        return services;
    }

    private static string? GetSecret(IConfiguration configuration, params string[] keys)
    {
        foreach (var key in keys)
        {
            var value = configuration[key];
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return null;
    }

    private static bool IsConfigured(string? clientId, string? clientSecret)
    {
        return !string.IsNullOrWhiteSpace(clientId)
            && !string.IsNullOrWhiteSpace(clientSecret)
            && !clientId.Contains("REPLACE-WITH", StringComparison.OrdinalIgnoreCase)
            && !clientSecret.Contains("REPLACE-WITH", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<GitHubProfileInfo?> FetchGitHubProfileAsync(string accessToken, string userInformationEndpoint)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        client.DefaultRequestHeaders.UserAgent.ParseAdd("CAPNEXA");

        var userJson = await GetJsonAsync(client, userInformationEndpoint);
        if (userJson is not { } userRoot || userRoot.ValueKind != JsonValueKind.Object)
            return null;

        var providerKey = userRoot.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.Number
            ? idProp.GetInt64().ToString(System.Globalization.CultureInfo.InvariantCulture)
            : null;

        if (providerKey is null)
            return null;

        var login = userRoot.TryGetProperty("login", out var loginProp) && loginProp.ValueKind == JsonValueKind.String
            ? loginProp.GetString()
            : null;
        var name = userRoot.TryGetProperty("name", out var nameProp) && nameProp.ValueKind == JsonValueKind.String
            ? nameProp.GetString()
            : null;
        var displayName = !string.IsNullOrWhiteSpace(name) ? name : login;

        string? verifiedEmail = null;
        string? anyEmail = null;

        if (await GetJsonAsync(client, "https://api.github.com/user/emails") is { } emailsRoot &&
            emailsRoot.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in emailsRoot.EnumerateArray())
            {
                if (!element.TryGetProperty("email", out var emailProp) || emailProp.ValueKind != JsonValueKind.String)
                    continue;

                var email = emailProp.GetString();
                if (string.IsNullOrWhiteSpace(email))
                    continue;

                anyEmail ??= email;

                var isVerified = element.TryGetProperty("verified", out var verifiedProp) && verifiedProp.ValueKind == JsonValueKind.True;
                if (!isVerified)
                    continue;

                var isPrimary = element.TryGetProperty("primary", out var primaryProp) && primaryProp.ValueKind == JsonValueKind.True;

                if (isPrimary)
                {
                    verifiedEmail = email;
                    break;
                }

                verifiedEmail ??= email;
            }
        }

        return new GitHubProfileInfo(providerKey, displayName, verifiedEmail, anyEmail);
    }

    private static async Task<JsonElement?> GetJsonAsync(HttpClient client, string url)
    {
        try
        {
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode)
                return null;

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);
            return document.RootElement.Clone();
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static void RedirectFrontendWithError(HttpContext httpContext, string error)
    {
        httpContext.Response.Redirect(BuildFrontendErrorUrl(httpContext, error));
    }

    private static string BuildFrontendErrorUrl(HttpContext httpContext, string error)
    {
        var configuration = httpContext.RequestServices.GetRequiredService<IConfiguration>();
        var frontendBase = configuration["AppUrls:FrontendBaseUrl"]?.TrimEnd('/')
            ?? $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
        var callbackPath = configuration["Frontend:OAuthCallbackPath"] ?? "/oauth/callback";

        return $"{frontendBase}{callbackPath}?error={Uri.EscapeDataString(error)}";
    }

    private sealed record GitHubProfileInfo(
        string ProviderKey,
        string? DisplayName,
        string? VerifiedEmail,
        string? AnyEmail);

    private static string BuildSpaRedirect(HttpContext httpContext, ExternalLoginResult result)
    {
        var configuration = httpContext.RequestServices.GetRequiredService<IConfiguration>();
        var frontendBase = configuration["AppUrls:FrontendBaseUrl"]?.TrimEnd('/')
            ?? $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
        var callbackPath = configuration["Frontend:OAuthCallbackPath"] ?? "/oauth/callback";

        var url = $"{frontendBase}{callbackPath}";

        return result.Succeeded
            ? $"{url}?code={Uri.EscapeDataString(result.ExchangeCode!)}"
            : $"{url}?error={Uri.EscapeDataString(result.ErrorCode!)}";
    }
}