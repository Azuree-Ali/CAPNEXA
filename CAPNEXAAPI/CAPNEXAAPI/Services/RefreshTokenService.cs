using CAPNEXAAPI.DTOs.Response;
using CAPNEXAAPI.Models;
using CAPNEXAAPI.Repositories;
using CAPNEXAAPI.Utils;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace CAPNEXAAPI.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IRepository<RefreshToken> _refreshTokens;
    private readonly ITokenService _tokenService;
    private readonly JwtSettings _settings;

    public RefreshTokenService(
        IRepository<RefreshToken> refreshTokens,
        ITokenService tokenService,
        IOptions<JwtSettings> settings)
    {
        _refreshTokens = refreshTokens;
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

        await _refreshTokens.CreateAsync(entity);
        await _refreshTokens.CommitAsync();

        return rawToken;
    }

    public async Task<(ApplicationUser User, string AccessToken, int ExpiresInSeconds, string RefreshToken)?> RefreshAsync(string rawToken)
    {
        var stored = await _refreshTokens.GetOneAsync(
            token => token.TokenHash == HashToken(rawToken),
            [token => token.User]);

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

        if (!stored.User.IsActive || !stored.User.EmailConfirmed)
        {
            stored.RevokedAtUtc = now;
            _refreshTokens.Update(stored);
            await _refreshTokens.CommitAsync();
            return null;
        }

        stored.RevokedAtUtc = now;

        var newRawToken = GenerateToken();
        var newHash = HashToken(newRawToken);
        stored.ReplacedByTokenHash = newHash;

        var (accessToken, expiresInSeconds, jwtId) = await _tokenService.CreateAccessTokenAsync(stored.User);

        _refreshTokens.Update(stored);
        await _refreshTokens.CreateAsync(new RefreshToken
        {
            UserId = stored.UserId,
            TokenHash = newHash,
            JwtId = jwtId,
            ExpiresAtUtc = now.AddDays(_settings.RefreshTokenExpiresInDays)
        });

        await _refreshTokens.CommitAsync();

        return (stored.User, accessToken, expiresInSeconds, newRawToken);
    }

    public async Task RevokeAllAsync(string userId)
    {
        var activeTokens = await _refreshTokens.GetAllAsync(
            token => token.UserId == userId && token.RevokedAtUtc == null);

        var revokedAt = DateTime.UtcNow;
        foreach (var token in activeTokens)
        {
            token.RevokedAtUtc = revokedAt;
            _refreshTokens.Update(token);
        }

        if (activeTokens.Any())
            await _refreshTokens.CommitAsync();
    }

    private async Task RevokeFamilyAsync(string userId)
    {
        var activeTokens = await _refreshTokens.GetAllAsync(
            token => token.UserId == userId && token.RevokedAtUtc == null);

        var revokedAt = DateTime.UtcNow;
        foreach (var token in activeTokens)
        {
            token.RevokedAtUtc = revokedAt;
            _refreshTokens.Update(token);
        }

        if (activeTokens.Any())
            await _refreshTokens.CommitAsync();
    }

    private static string GenerateToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private static string HashToken(string rawToken)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken))).ToLowerInvariant();
}
