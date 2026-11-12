using CAPNEXAAPI.DTOs.Request;
using CAPNEXAAPI.DTOs.Response;

namespace CAPNEXAAPI.Services;

public interface IAuthService
{
    Task<AuthOperationResult> RegisterAsync(RegisterDto request, string apiBaseUrl);
    Task<AuthOperationResult> ResendConfirmationAsync(string email, string apiBaseUrl);
    Task<bool> ConfirmEmailAsync(string userId, string token);
    Task<AuthOperationResult<AuthResponseDto>> LoginAsync(LoginDto request);
    Task<AuthOperationResult<AuthResponseDto>> ExternalLoginAsync(
        string provider,
        string providerKey,
        string? email,
        bool emailVerified,
        string? firstName,
        string? lastName,
        string? displayName);
    Task<AuthOperationResult<AuthResponseDto>> RefreshAsync(RefreshTokenDto request);
}

public sealed record AuthOperationResult<T>(T? Value, string? Error = null, bool Unauthorized = false)
{
    public bool Succeeded => Error is null;
}

public sealed record AuthOperationResult(
    string? Error = null,
    string? Message = null,
    bool Conflict = false,
    bool EmailConfirmationSent = true,
    bool ServiceUnavailable = false)
{
    public bool Succeeded => Error is null;
}
