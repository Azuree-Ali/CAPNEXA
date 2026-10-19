using System.Collections.Concurrent;
using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;

namespace CAPNEXAAPI.Services;

public class ExchangeCodeService
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(2);
    private readonly ConcurrentDictionary<string, ExchangeCodeEntry> _codes = new();

    public string Issue(string userId)
    {
        var code = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        _codes[Hash(code)] = new ExchangeCodeEntry(userId, DateTimeOffset.UtcNow.Add(Lifetime));
        return code;
    }

    public bool TryConsume(string code, out string userId)
    {
        userId = string.Empty;

        // TryRemove is atomic: at most one concurrent request can redeem a code.
        if (!_codes.TryRemove(Hash(code), out var entry) || entry.ExpiresAt <= DateTimeOffset.UtcNow)
            return false;

        userId = entry.UserId;
        return !string.IsNullOrWhiteSpace(userId);
    }

    private static string Hash(string value)
        => Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value)));

    private sealed record ExchangeCodeEntry(string UserId, DateTimeOffset ExpiresAt);
}
