namespace CAPNEXAAPI.DTOs.Response;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresInSeconds { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public UserResponseDto User { get; set; } = null!;
}