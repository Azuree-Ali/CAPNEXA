using System.Net;
using CAPNEXAAPI.DataAccess;
using CAPNEXAAPI.DTOs.Request;
using CAPNEXAAPI.DTOs.Response;
using CAPNEXAAPI.Models;
using CAPNEXAAPI.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace CAPNEXAAPI.Services;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly ApplicationDbContext _db;
    private readonly IEmailSender _emailSender;
    private readonly ITokenService _tokens;
    private readonly IRefreshTokenService _refreshTokens;
    private readonly ExchangeCodeService _exchangeCodes;

    public AuthService(
        UserManager<ApplicationUser> users,
        ApplicationDbContext db,
        IEmailSender emailSender,
        ITokenService tokens,
        IRefreshTokenService refreshTokens,
        ExchangeCodeService exchangeCodes)
    {
        _users = users;
        _db = db;
        _emailSender = emailSender;
        _tokens = tokens;
        _refreshTokens = refreshTokens;
        _exchangeCodes = exchangeCodes;
    }

    public async Task<AuthOperationResult> RegisterAsync(RegisterDto request, string apiBaseUrl)
    {
        if (await _users.FindByEmailAsync(request.Email) is not null)
            return new("This email address is already in use.", Conflict: true);

        if (await _db.StudentProfiles.AnyAsync(s => s.StudentNumber == request.StudentNumber))
            return new("This university ID is already in use.", Conflict: true);

        var user = new ApplicationUser
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim(),
            UserName = request.Email.Trim()
        };

        await using var transaction = await _db.Database.BeginTransactionAsync();
        var create = await _users.CreateAsync(user, request.Password);
        if (!create.Succeeded)
        {
            await transaction.RollbackAsync();
            return new(string.Join(" ", create.Errors.Select(e => e.Description)));
        }

        var roleResult = await _users.AddToRoleAsync(user, CD.STUDENT_ROLE);
        if (!roleResult.Succeeded)
        {
            await transaction.RollbackAsync();
            return new(string.Join(" ", roleResult.Errors.Select(e => e.Description)));
        }

        _db.StudentProfiles.Add(new StudentProfile
        {
            UserId = user.Id,
            StudentNumber = request.StudentNumber.Trim(),
            Department = request.Department.Trim(),
            AcademicYear = request.AcademicYear.Trim()
        });

        try
        {
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();
            _db.Entry(user).State = EntityState.Detached;
            var profileEntry = _db.ChangeTracker.Entries<StudentProfile>()
                .FirstOrDefault(entry => entry.Entity.UserId == user.Id);
            if (profileEntry is not null)
                profileEntry.State = EntityState.Detached;

            if (await _db.StudentProfiles.AnyAsync(s => s.StudentNumber == request.StudentNumber))
                return new("This university ID is already in use.", Conflict: true);

            throw;
        }

        await SendConfirmationEmailAsync(user, apiBaseUrl);
        return new(Message: "Registration completed successfully. Please check your email to activate your account.");
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
        var user = await _users.FindByEmailAsync(request.Email);
        if (user is null || !await _users.CheckPasswordAsync(user, request.Password))
            return new(null, "Invalid email or password.", Unauthorized: true);
        if (!user.IsActive)
            return new(null, "Your account has been disabled.", Unauthorized: true);
        if (!await _users.IsEmailConfirmedAsync(user))
            return new(null, "Your account is not activated yet. Please activate your account first.");

        return new(await BuildAuthResponseAsync(user), null);
    }

    public async Task<AuthOperationResult<AuthResponseDto>> ExchangeAsync(ExchangeCodeDto request)
    {
        if (!_exchangeCodes.TryConsume(request.Code, out var userId))
            return new(null, "Invalid or expired authorization code.");

        var user = await _users.FindByIdAsync(userId);
        if (user is null || !user.IsActive || !await _users.IsEmailConfirmedAsync(user))
            return new(null, "Unable to sign in with this code.");

        return new(await BuildAuthResponseAsync(user), null);
    }

    public async Task<AuthOperationResult<AuthResponseDto>> RefreshAsync(RefreshTokenDto request)
    {
        var result = await _refreshTokens.RefreshAsync(request.RefreshToken);
        if (result is null)
            return new(null, "Invalid, expired, or reused refresh token.");

        return new(new AuthResponseDto
        {
            AccessToken = result.Value.AccessToken,
            ExpiresInSeconds = result.Value.ExpiresInSeconds,
            RefreshToken = result.Value.RefreshToken,
            User = await BuildUserResponseAsync(result.Value.User)
        }, null);
    }

    public Task<bool> RevokeAsync(string refreshToken, string userId)
        => _refreshTokens.RevokeAsync(refreshToken, userId);

    private async Task<AuthResponseDto> BuildAuthResponseAsync(ApplicationUser user)
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
        var link = $"{apiBaseUrl.TrimEnd('/')}/api/Auth/confirm-email" +
                   $"?userId={WebUtility.UrlEncode(user.Id)}&token={WebUtility.UrlEncode(token)}";
        var body = $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto">
              <h2>Welcome to CAPNEXA</h2>
              <p>Hi {WebUtility.HtmlEncode(user.FirstName)},</p>
              <p>Thanks for registering. Please confirm your email address to activate your account.</p>
              <p><a href="{link}" style="display:inline-block;padding:10px 20px;background:#2563eb;color:#fff;text-decoration:none;border-radius:6px">Activate my account</a></p>
              <p>If the button does not work, copy and paste this link into your browser:</p>
              <p><a href="{link}">{link}</a></p>
            </div>
            """;
        await _emailSender.SendEmailAsync(user.Email!, "CAPNEXA - Activate your account", body);
    }
}
