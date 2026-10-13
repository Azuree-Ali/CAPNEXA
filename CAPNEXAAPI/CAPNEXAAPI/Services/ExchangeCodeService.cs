using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;

namespace CAPNEXAAPI.Services;

public class ExchangeCodeService
{
    private const string Purpose = "Auth.ExchangeCode.v1";
    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(2);

    private readonly IDataProtector _protector;
    private readonly IMemoryCache _cache;

    public ExchangeCodeService(IDataProtectionProvider protectionProvider, IMemoryCache cache)
    {
        _protector = protectionProvider.CreateProtector(Purpose);
        _cache = cache;
    }

    public string Issue(string userId)
    {
        var nonce = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
        var payload = string.Join('|', userId, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), nonce);
        var code = _protector.Protect(payload);

        _cache.Set(code, userId, Lifetime);

        return code;
    }

    public bool TryConsume(string code, out string userId)
    {
        userId = string.Empty;

        try
        {
            var payload = _protector.Unprotect(code);
            var parts = payload.Split('|');

            if (parts.Length != 3 || !long.TryParse(parts[1], out var issuedAt))
                return false;

            if (DateTimeOffset.FromUnixTimeSeconds(issuedAt).Add(Lifetime) < DateTimeOffset.UtcNow)
                return false;

            if (!_cache.TryGetValue(code, out userId!))
                return false;

            _cache.Remove(code);
            return !string.IsNullOrWhiteSpace(userId);
        }
        catch (CryptographicException)
        {
            return false;
        }
    }
}