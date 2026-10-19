using CAPNEXAAPI.DataAccess;
using CAPNEXAAPI.Models;
using CAPNEXAAPI.Services;
using CAPNEXAAPI.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;
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
            .AddCookie(IdentityConstants.ExternalScheme, options =>
            {
                options.Cookie.Name = IdentityConstants.ExternalScheme;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
            });

        var googleClientId = configuration["Auth:Google:ClientId"];
        var googleClientSecret = configuration["Auth:Google:ClientSecret"];
        var googleCallbackPath = configuration["Auth:Google:CallbackPath"] ?? "/api/auth/google/callback";

        if (IsConfigured(googleClientId, googleClientSecret))
        {
            authBuilder.AddOpenIdConnect("Google", options =>
            {
                options.Authority = "https://accounts.google.com";
                options.ClientId = googleClientId!;
                options.ClientSecret = googleClientSecret!;
                options.CallbackPath = googleCallbackPath;
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.ResponseType = "code";
                options.ResponseMode = "query";
                options.UsePkce = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.SaveTokens = false;
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");

                options.Events = new OpenIdConnectEvents
                {
                    OnRedirectToIdentityProvider = context =>
                    {
                        context.ProtocolMessage.RedirectUri = BuildApiCallbackUrl(
                            context.HttpContext, context.Options.CallbackPath);
                        context.ProtocolMessage.Prompt = "select_account";
                        return Task.CompletedTask;
                    },
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
                    },
                    OnRemoteFailure = context =>
                    {
                        return HandleExternalAuthFailureAsync(context, "Google");
                    }
                };
            });
        }

        var githubClientId = configuration["Auth:GitHub:ClientId"];
        var githubClientSecret = configuration["Auth:GitHub:ClientSecret"];
        var githubCallbackPath = configuration["Auth:GitHub:CallbackPath"] ?? "/api/auth/github/callback";

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
                options.Events = new OAuthEvents
                {
                    OnRedirectToAuthorizationEndpoint = context =>
                    {
                        // Keep the OAuth handler's authorization URL (including state,
                        // PKCE, client_id, and scopes), while pinning only redirect_uri
                        // to the configured API URL instead of trusting the request Host.
                        var authorizationUri = new Uri(context.RedirectUri);
                        var query = QueryHelpers.ParseQuery(authorizationUri.Query)
                            .Where(parameter => !string.Equals(
                                parameter.Key, "redirect_uri", StringComparison.OrdinalIgnoreCase) &&
                                !string.Equals(parameter.Key, "prompt", StringComparison.OrdinalIgnoreCase))
                            .ToDictionary(parameter => parameter.Key, parameter => (string?)parameter.Value.ToString());
                        query["redirect_uri"] = BuildApiCallbackUrl(
                            context.HttpContext, context.Options.CallbackPath);
                        query["prompt"] = "select_account";

                        var authorizationUrl = QueryHelpers.AddQueryString(
                            authorizationUri.GetLeftPart(UriPartial.Path), query);
                        context.Response.Redirect(authorizationUrl);
                        return Task.CompletedTask;
                    },
                    OnCreatingTicket = async context =>
                    {
                        var profile = context.AccessToken is { Length: > 0 } accessToken
                            ? await FetchGitHubProfileAsync(
                                context.HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>(),
                                accessToken,
                                context.Options.UserInformationEndpoint)
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
                        return HandleExternalAuthFailureAsync(context, "GitHub");
                    }
                };
            });
        }

        services.AddAuthorization();

        services.AddCors(options =>
        {
            var frontendBaseUrl = configuration["AppUrls:FrontendBaseUrl"];
            if (!Uri.TryCreate(frontendBaseUrl, UriKind.Absolute, out var frontendUri) ||
                (frontendUri.Scheme != Uri.UriSchemeHttp && frontendUri.Scheme != Uri.UriSchemeHttps))
            {
                throw new InvalidOperationException("'AppUrls:FrontendBaseUrl' must be an absolute HTTP or HTTPS URL.");
            }

            options.AddPolicy("Frontend", policy =>
            {
                policy.WithOrigins(frontendUri.GetLeftPart(UriPartial.Authority))
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

        services.AddSingleton<ExchangeCodeService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IExternalAuthService, ExternalAuthService>();

        services.AddHttpClient("GitHub", client => client.Timeout = TimeSpan.FromSeconds(15));
        services.AddTransient<IEmailSender, EmailSender>();

        return services;
    }

    private static bool IsConfigured(string? clientId, string? clientSecret)
    {
        return !string.IsNullOrWhiteSpace(clientId)
            && !string.IsNullOrWhiteSpace(clientSecret)
            && !clientId.Contains("REPLACE-WITH", StringComparison.OrdinalIgnoreCase)
            && !clientSecret.Contains("REPLACE-WITH", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<GitHubProfileInfo?> FetchGitHubProfileAsync(
        IHttpClientFactory httpClientFactory,
        string accessToken,
        string userInformationEndpoint)
    {
        using var client = httpClientFactory.CreateClient("GitHub");

        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        client.DefaultRequestHeaders.UserAgent.ParseAdd("CAPNEXA");

        var userJson = await GetJsonAsync(client, userInformationEndpoint, accessToken);
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

        if (await GetJsonAsync(client, "https://api.github.com/user/emails", accessToken) is { } emailsRoot &&
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

    private static async Task<JsonElement?> GetJsonAsync(HttpClient client, string url, string accessToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode)
                return null;

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);
            return document.RootElement.Clone();
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (TaskCanceledException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static Task HandleExternalAuthFailureAsync(RemoteFailureContext context, string provider)
    {
        var logger = context.HttpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("CAPNEXAAPI.ExternalAuthentication");
        var failureType = context.Failure?.GetType().Name ?? "UnknownFailure";
        // Provider responses can arrive in the callback query or inside the
        // framework exception (for example, OIDC token endpoint invalid_client).
        // Only expose a small allowlist of error codes; never log exception text.
        var failureText = context.Failure?.ToString() ?? string.Empty;
        var providerError = GetSafeProviderErrorCode(
            context.HttpContext.Request.Query["error"].ToString(), failureText);

        logger.LogWarning(
            "External authentication failed for {Provider}. FailureType={FailureType}, ProviderError={ProviderError}",
            provider,
            failureType,
            string.IsNullOrEmpty(providerError) ? "none" : providerError);

        var redirectUrl = BuildFrontendErrorUrl(context.HttpContext, "oauth_failed");
        if (context.HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
        {
            redirectUrl = QueryHelpers.AddQueryString(redirectUrl, new Dictionary<string, string?>
            {
                ["provider"] = provider.ToLowerInvariant(),
                ["reason"] = string.IsNullOrEmpty(providerError) ? failureType : providerError
            });
        }

        context.HttpContext.Response.Redirect(redirectUrl);
        context.HandleResponse();
        return Task.CompletedTask;
    }

    private static string GetSafeProviderErrorCode(string callbackError, string failureText)
    {
        string[] safeCodes =
        [
            "invalid_client", "invalid_grant", "invalid_request", "invalid_scope",
            "access_denied", "unauthorized_client", "redirect_uri_mismatch",
            "temporarily_unavailable", "server_error"
        ];

        foreach (var code in safeCodes)
        {
            if (string.Equals(callbackError, code, StringComparison.OrdinalIgnoreCase) ||
                failureText.Contains(code, StringComparison.OrdinalIgnoreCase))
            {
                return code;
            }
        }

        return string.Empty;
    }

    private static string BuildFrontendErrorUrl(HttpContext httpContext, string error)
    {
        var configuration = httpContext.RequestServices.GetRequiredService<IConfiguration>();
        var callback = GetFrontendCallbackUrl(configuration);
        return QueryHelpers.AddQueryString(callback, "error", error);
    }

    private sealed record GitHubProfileInfo(
        string ProviderKey,
        string? DisplayName,
        string? VerifiedEmail,
        string? AnyEmail);

    private static string BuildSpaRedirect(HttpContext httpContext, ExternalLoginResult result)
    {
        var configuration = httpContext.RequestServices.GetRequiredService<IConfiguration>();
        var url = GetFrontendCallbackUrl(configuration);

        return result.Succeeded
            ? $"{url}#code={Uri.EscapeDataString(result.ExchangeCode!)}"
            : QueryHelpers.AddQueryString(url, "error", result.ErrorCode ?? "oauth_failed");
    }

    private static string BuildApiCallbackUrl(HttpContext httpContext, PathString callbackPath)
    {
        var configuration = httpContext.RequestServices.GetRequiredService<IConfiguration>();
        var apiBaseUrl = configuration["AppUrls:ApiBaseUrl"];
        var path = callbackPath.Value;
        if (!Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out var apiUri) ||
            apiUri.Scheme != Uri.UriSchemeHttps ||
            !string.IsNullOrEmpty(apiUri.UserInfo) ||
            !string.IsNullOrEmpty(apiUri.Query) ||
            !string.IsNullOrEmpty(apiUri.Fragment) ||
            apiUri.AbsolutePath != "/" ||
            string.IsNullOrWhiteSpace(path) || path[0] != '/' ||
            path.StartsWith("//", StringComparison.Ordinal) ||
            path.Contains('?') || path.Contains('#'))
        {
            throw new InvalidOperationException("OAuth API callback configuration is invalid.");
        }

        return new Uri(apiUri.GetLeftPart(UriPartial.Authority) + path).AbsoluteUri;
    }

    private static string GetFrontendCallbackUrl(IConfiguration configuration)
    {
        var frontendBase = configuration["AppUrls:FrontendBaseUrl"];
        var callbackPath = configuration["Frontend:OAuthCallbackPath"] ?? "/oauth/callback";
        if (!Uri.TryCreate(frontendBase, UriKind.Absolute, out var frontendUri) ||
            (frontendUri.Scheme != Uri.UriSchemeHttp && frontendUri.Scheme != Uri.UriSchemeHttps) ||
            !string.IsNullOrEmpty(frontendUri.UserInfo) ||
            !string.IsNullOrEmpty(frontendUri.Query) ||
            !string.IsNullOrEmpty(frontendUri.Fragment) ||
            frontendUri.AbsolutePath != "/" ||
            callbackPath.Length == 0 || callbackPath[0] != '/' ||
            callbackPath.StartsWith("//", StringComparison.Ordinal) ||
            callbackPath.Contains('?') ||
            callbackPath.Contains('#'))
        {
            throw new InvalidOperationException("OAuth frontend redirect configuration is invalid.");
        }

        // Only a configured relative path is accepted; request parameters can never
        // choose an external redirect destination.
        return new Uri(frontendUri.GetLeftPart(UriPartial.Authority) + callbackPath).AbsoluteUri;
    }
}
