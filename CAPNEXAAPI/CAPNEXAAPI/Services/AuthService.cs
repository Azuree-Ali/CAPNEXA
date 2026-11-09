using System.Net;
using System.Net.Mail;
using CAPNEXAAPI.DTOs.Request;
using CAPNEXAAPI.DTOs.Response;
using CAPNEXAAPI.Models;
using CAPNEXAAPI.Repositories;
using CAPNEXAAPI.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace CAPNEXAAPI.Services;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly IRepository<StudentProfile> _studentProfiles;
    private readonly IEmailSender _emailSender;
    private readonly ITokenService _tokens;
    private readonly IRefreshTokenService _refreshTokens;

    public AuthService(
        UserManager<ApplicationUser> users,
        IRepository<StudentProfile> studentProfiles,
        IEmailSender emailSender,
        ITokenService tokens,
        IRefreshTokenService refreshTokens)
    {
        _users = users;
        _studentProfiles = studentProfiles;
        _emailSender = emailSender;
        _tokens = tokens;
        _refreshTokens = refreshTokens;
    }

    public async Task<AuthOperationResult> RegisterAsync(RegisterDto request, string apiBaseUrl)
    {
        var email = request.Email.Trim();
        var studentNumber = request.StudentNumber.Trim();

        if (await _users.FindByEmailAsync(email) is not null)
            return new("This email address is already in use.", Conflict: true);

        if (await _studentProfiles.GetOneAsync(profile => profile.StudentNumber == studentNumber) is not null)
            return new("This student number is already in use.", Conflict: true);

        var user = new ApplicationUser
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            UserName = email
        };

        var createResult = await _users.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
            return new(string.Join(" ", createResult.Errors.Select(error => error.Description)));

        var roleResult = await _users.AddToRoleAsync(user, CD.STUDENT_ROLE);
        if (!roleResult.Succeeded)
        {
            await _users.DeleteAsync(user);
            return new("The account could not be registered.");
        }

        await _studentProfiles.CreateAsync(new StudentProfile
        {
            UserId = user.Id,
            StudentNumber = studentNumber,
            Department = request.Department.Trim(),
            AcademicYear = request.AcademicYear.Trim()
        });
        await _studentProfiles.CommitAsync();

        try
        {
            await SendConfirmationEmailAsync(user, apiBaseUrl);
            return new(Message: "Account created. Check your email and confirm it before logging in.");
        }
        catch (SmtpException)
        {
            return new(
                Message: "Account created, but the confirmation email was not sent. Request another confirmation email.",
                EmailConfirmationSent: false);
        }
        catch (InvalidOperationException)
        {
            return new(
                Message: "Account created, but the confirmation email was not sent. Request another confirmation email.",
                EmailConfirmationSent: false);
        }
    }

    public async Task<AuthOperationResult> ResendConfirmationAsync(string email, string apiBaseUrl)
    {
        var user = await _users.FindByEmailAsync(email.Trim());
        if (user is null || user.EmailConfirmed || !user.IsActive)
            return new(Message: "If the account needs confirmation, a link will be sent.");

        try
        {
            await SendConfirmationEmailAsync(user, apiBaseUrl);
            return new(Message: "If the account needs confirmation, a link will be sent.");
        }
        catch (SmtpException)
        {
            return new("The confirmation email could not be sent. Try again later.", ServiceUnavailable: true);
        }
        catch (InvalidOperationException)
        {
            return new("The confirmation email could not be sent. Try again later.", ServiceUnavailable: true);
        }
    }

    public async Task<bool> ConfirmEmailAsync(string userId, string token)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user is null)
            return false;

        var result = await _users.ConfirmEmailAsync(user, token);
        return result.Succeeded;
    }

    public async Task<AuthOperationResult<AuthResponseDto>> LoginAsync(LoginDto request)
    {
        var user = await _users.FindByEmailAsync(request.Email.Trim());
        if (user is null || !await _users.CheckPasswordAsync(user, request.Password))
            return new(null, "Invalid email or password.", Unauthorized: true);

        if (!user.IsActive)
            return new(null, "This account is disabled.", Unauthorized: true);

        if (!await _users.IsEmailConfirmedAsync(user))
            return new(null, "Confirm your email before logging in.");

        return new(await CreateAuthResponseAsync(user));
    }

    public async Task<AuthOperationResult<AuthResponseDto>> ExternalLoginAsync(
        string provider,
        string providerKey,
        string? email,
        bool emailVerified,
        string? firstName,
        string? lastName,
        string? displayName)
    {
        if (string.IsNullOrWhiteSpace(providerKey))
            return new(null, "invalid_provider_id");

        var user = await _users.FindByLoginAsync(provider, providerKey);
        if (user is not null)
        {
            if (!user.IsActive)
                return new(null, "account_disabled");
            if (!await _users.IsEmailConfirmedAsync(user))
                return new(null, "email_unconfirmed");

            return new(await CreateAuthResponseAsync(user));
        }

        if (string.IsNullOrWhiteSpace(email))
            return new(null, "email_required");
        if (!emailVerified)
            return new(null, "email_unverified");
        if (await _users.FindByEmailAsync(email) is not null)
            return new(null, "email_conflict");

        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = FirstNameOrDefault(firstName, displayName, provider),
            LastName = lastName ?? string.Empty,
            IsActive = true
        };

        var createResult = await _users.CreateAsync(user);
        if (!createResult.Succeeded)
            return new(null, "external_account_create_failed");

        var roleResult = await _users.AddToRoleAsync(user, CD.STUDENT_ROLE);
        if (!roleResult.Succeeded)
        {
            await _users.DeleteAsync(user);
            return new(null, "external_account_create_failed");
        }

        var loginResult = await _users.AddLoginAsync(user, new UserLoginInfo(provider, providerKey, provider));
        if (!loginResult.Succeeded)
        {
            await _users.DeleteAsync(user);
            return new(null, "external_account_create_failed");
        }

        // External users complete their academic profile after signing in.
        return new(await CreateAuthResponseAsync(user));
    }

    public async Task<AuthOperationResult<AuthResponseDto>> RefreshAsync(RefreshTokenDto request)
    {
        var result = await _refreshTokens.RefreshAsync(request.RefreshToken);
        if (result is null)
            return new(null, "Refresh token is invalid, expired, or already used.", Unauthorized: true);

        return new(new AuthResponseDto
        {
            AccessToken = result.Value.AccessToken,
            ExpiresInSeconds = result.Value.ExpiresInSeconds,
            RefreshToken = result.Value.RefreshToken,
            User = await BuildUserResponseAsync(result.Value.User)
        });
    }

    private async Task<AuthResponseDto> CreateAuthResponseAsync(ApplicationUser user)
    {
        var (accessToken, expiresInSeconds, jwtId) = await _tokens.CreateAccessTokenAsync(user);
        var refreshToken = await _refreshTokens.IssueAsync(user.Id, jwtId);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            ExpiresInSeconds = expiresInSeconds,
            RefreshToken = refreshToken,
            User = await BuildUserResponseAsync(user)
        };
    }

    private async Task<UserResponseDto> BuildUserResponseAsync(ApplicationUser user)
        => new()
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = await _users.GetRolesAsync(user)
        };

    private async Task SendConfirmationEmailAsync(ApplicationUser user, string apiBaseUrl)
    {
        var token = await _users.GenerateEmailConfirmationTokenAsync(user);
        var link = $"{apiBaseUrl.TrimEnd('/')}/api/Identity/Auth/confirm-email" +
                   $"?userId={WebUtility.UrlEncode(user.Id)}&token={WebUtility.UrlEncode(token)}";
        var body = $"<h2>Welcome to CAPNEXA</h2><p>Hi {WebUtility.HtmlEncode(user.FirstName)},</p>" +
                   $"<p><a href=\"{link}\">Confirm your email and activate your account</a></p>";

        await _emailSender.SendEmailAsync(user.Email!, "CAPNEXA - Confirm your email", body);
    }

    private static string FirstNameOrDefault(string? firstName, string? displayName, string provider)
    {
        if (!string.IsNullOrWhiteSpace(firstName))
            return firstName;

        return string.IsNullOrWhiteSpace(displayName) ? provider : displayName;
    }
}
