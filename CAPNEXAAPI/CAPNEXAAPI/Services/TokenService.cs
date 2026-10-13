using CAPNEXAAPI.Models;
using CAPNEXAAPI.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CAPNEXAAPI.Services;

public class TokenService : ITokenService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtSettings _settings;

    public TokenService(UserManager<ApplicationUser> userManager, IOptions<JwtSettings> settings)
    {
        _userManager = userManager;
        _settings = settings.Value;
    }

    public async Task<(string AccessToken, int ExpiresInSeconds, string JwtId)> CreateAccessTokenAsync(ApplicationUser user)
    {
        if (string.IsNullOrWhiteSpace(_settings.Key) || _settings.Key.Length < 32)
        {
            throw new InvalidOperationException(
                "The JWT signing key is not configured. Set 'Jwt:Key' (at least 32 characters) using user-secrets or the JWT__KEY environment variable.");
        }

        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(_settings.AccessTokenExpiresInMinutes);
        var jwtId = Guid.NewGuid().ToString("N");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, jwtId),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, string.Join(' ', user.FirstName, user.LastName).Trim())
        };

        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: credentials);

        var handler = new JwtSecurityTokenHandler();
        return (handler.WriteToken(token), (int)(expires - now).TotalSeconds, jwtId);
    }
}