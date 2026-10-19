using CAPNEXAAPI.DTOs.Request;
using CAPNEXAAPI.DTOs.Response;

namespace CAPNEXAAPI.Services;

public interface IAuthService
{
    Task<AuthOperationResult> RegisterAsync(RegisterDto request, string apiBaseUrl);
    Task<bool> ConfirmEmailAsync(string userId, string token);
    Task<AuthOperationResult<AuthResponseDto>> LoginAsync(LoginDto request);
    Task<AuthOperationResult<AuthResponseDto>> ExchangeAsync(ExchangeCodeDto request);
    Task<AuthOperationResult<AuthResponseDto>> RefreshAsync(RefreshTokenDto request);
    Task<bool> RevokeAsync(string refreshToken, string userId);
}

public sealed record AuthOperationResult<T>(T? Value, string? Error, bool Unauthorized = false)
{
    public bool Succeeded => Error is null;
}

public sealed record AuthOperationResult(string? Error = null, bool Conflict = false, string? Message = null)
{
    public bool Succeeded => Error is null;
}
