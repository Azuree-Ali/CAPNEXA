using CAPNEXAAPI.Models;

namespace CAPNEXAAPI.Services;

public interface IRefreshTokenService
{
    Task<string> IssueAsync(string userId, string? jwtId = null);
    Task<(ApplicationUser User, string AccessToken, int ExpiresInSeconds, string RefreshToken)?> RefreshAsync(string rawToken);
    Task<bool> RevokeAsync(string rawToken, string userId);
    Task RevokeAllAsync(string userId);
}
