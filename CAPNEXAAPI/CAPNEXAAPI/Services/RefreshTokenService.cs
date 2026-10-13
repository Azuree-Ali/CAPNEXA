using CAPNEXAAPI.DataAccess;
using CAPNEXAAPI.DTOs.Response;
using CAPNEXAAPI.Models;
using CAPNEXAAPI.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace CAPNEXAAPI.Services;

public class RefreshTokenService
{
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly JwtSettings _settings;

    public RefreshTokenService(
        ApplicationDbContext context,
        ITokenService tokenService,
        IOptions<JwtSettings> settings)
    {
        _context = context;
        _tokenService = tokenService;
        _settings = settings.Value;
    }

    public async Task<string> IssueAsync(string userId, string? jwtId = null)
    {
        var rawToken = GenerateToken();
        var entity = new RefreshToken
        {
            UserId = userId,
            TokenHash = HashToken(rawToken),
            JwtId = jwtId,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpiresInDays)
        };

        _context.RefreshTokens.Add(entity);
        await _context.SaveChangesAsync();

        return rawToken;
    }

    public async Task<(ApplicationUser User, string AccessToken, int ExpiresInSeconds, string RefreshToken)?> RefreshAsync(string rawToken)
    {
        var stored = await _context.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == HashToken(rawToken));

        if (stored is null)
            return null;

        var now = DateTime.UtcNow;

        if (stored.RevokedAtUtc is not null || stored.ReplacedByTokenHash is not null)
        {
            await RevokeFamilyAsync(stored.UserId);
            return null;
        }

        if (stored.ExpiresAtUtc <= now)
            return null;

        await using var transaction = await _context.Database.BeginTransactionAsync();

        stored.RevokedAtUtc = now;

        var newRawToken = GenerateToken();
        var newHash = HashToken(newRawToken);
        stored.ReplacedByTokenHash = newHash;

        var (accessToken, expiresInSeconds, jwtId) = await _tokenService.CreateAccessTokenAsync(stored.User);

        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = stored.UserId,
            TokenHash = newHash,
            JwtId = jwtId,
            ExpiresAtUtc = now.AddDays(_settings.RefreshTokenExpiresInDays)
        });

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return (stored.User, accessToken, expiresInSeconds, newRawToken);
    }

    public async Task<bool> RevokeAsync(string rawToken, string userId)
    {
        var stored = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == HashToken(rawToken) && t.UserId == userId);

        if (stored is null || stored.RevokedAtUtc is not null)
            return false;

        stored.RevokedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    private async Task RevokeFamilyAsync(string userId)
    {
        var activeTokens = await _context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAtUtc == null)
            .ToListAsync();

        foreach (var token in activeTokens)
            token.RevokedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    private static string GenerateToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private static string HashToken(string rawToken)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken))).ToLowerInvariant();
}