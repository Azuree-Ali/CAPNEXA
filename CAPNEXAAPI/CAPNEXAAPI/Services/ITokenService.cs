using CAPNEXAAPI.Models;

namespace CAPNEXAAPI.Services;

public interface ITokenService
{
    Task<(string AccessToken, int ExpiresInSeconds, string JwtId)> CreateAccessTokenAsync(ApplicationUser user);
}