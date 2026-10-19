using CAPNEXAAPI.DataAccess;
using CAPNEXAAPI.Models;
using CAPNEXAAPI.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CAPNEXAAPI.Services;

public class ExternalAuthService : IExternalAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ExchangeCodeService _exchangeCodeService;

    public ExternalAuthService(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        ExchangeCodeService exchangeCodeService)
    {
        _userManager = userManager;
        _context = context;
        _exchangeCodeService = exchangeCodeService;
    }

    public async Task<ExternalLoginResult> ProcessAsync(
        string provider,
        string providerKey,
        string? email,
        bool emailVerified,
        string? firstName,
        string? lastName,
        string? displayName)
    {
        if (string.IsNullOrWhiteSpace(providerKey))
            return Fail("invalid_provider_id");

        var user = await _userManager.FindByLoginAsync(provider, providerKey);

        if (user is not null)
            return user.IsActive && await _userManager.IsEmailConfirmedAsync(user)
                ? Succeed(user)
                : Fail(user.IsActive ? "email_unconfirmed" : "account_disabled");

        var created = await CreateUserAsync(provider, providerKey, email, emailVerified, firstName, lastName, displayName);

        return created.User is not null
            ? Succeed(created.User)
            : Fail(created.ErrorCode ?? "creation_failed");
    }

    private ExternalLoginResult Succeed(ApplicationUser user)
        => new() { Succeeded = true, ExchangeCode = _exchangeCodeService.Issue(user.Id) };

    private static ExternalLoginResult Fail(string errorCode)
        => new() { Succeeded = false, ErrorCode = errorCode };

    private async Task<(ApplicationUser? User, string? ErrorCode)> CreateUserAsync(
        string provider,
        string providerKey,
        string? email,
        bool emailVerified,
        string? firstName,
        string? lastName,
        string? displayName)
    {
        if (string.IsNullOrWhiteSpace(email))
            return (null, "email_required");

        if (!emailVerified)
            return (null, "email_unverified");

        // Never link an external identity to an existing account solely because emails match.
        if (await _userManager.FindByEmailAsync(email) is not null)
            return (null, "email_conflict");

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = FirstNameOrDefault(firstName, displayName, provider),
            LastName = lastName ?? string.Empty,
            IsActive = true
        };

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                await transaction.RollbackAsync();
                _context.Entry(user).State = EntityState.Detached;
                return await ResolveAfterFailureAsync(provider, providerKey);
            }

            var addLoginResult = await _userManager.AddLoginAsync(
                user, new UserLoginInfo(provider, providerKey, provider));
            if (!addLoginResult.Succeeded)
            {
                await transaction.RollbackAsync();
                _context.Entry(user).State = EntityState.Detached;
                return await ResolveAfterFailureAsync(provider, providerKey);
            }

            if (!await _userManager.IsInRoleAsync(user, CD.STUDENT_ROLE))
            {
                var addRoleResult = await _userManager.AddToRoleAsync(user, CD.STUDENT_ROLE);
                if (!addRoleResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    _context.Entry(user).State = EntityState.Detached;
                    return await ResolveAfterFailureAsync(provider, providerKey);
                }
            }

            await transaction.CommitAsync();
            return (user, null);
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();
            _context.Entry(user).State = EntityState.Detached;
            return await ResolveAfterFailureAsync(provider, providerKey);
        }
    }

    private async Task<(ApplicationUser? User, string? ErrorCode)> ResolveAfterFailureAsync(string provider, string providerKey)
    {
        // A concurrent registration may have already created and linked this login.
        var existing = await _userManager.FindByLoginAsync(provider, providerKey);
        if (existing is not null && existing.IsActive && await _userManager.IsEmailConfirmedAsync(existing))
            return (existing, null);

        return (null, "creation_failed");
    }

    private static string FirstNameOrDefault(string? firstName, string? displayName, string provider)
    {
        if (!string.IsNullOrWhiteSpace(firstName))
            return firstName;

        return string.IsNullOrWhiteSpace(displayName) ? provider : displayName;
    }
}
