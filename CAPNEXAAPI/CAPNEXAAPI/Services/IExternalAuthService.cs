namespace CAPNEXAAPI.Services;

public class ExternalLoginResult
{
    public bool Succeeded { get; set; }
    public string? ErrorCode { get; set; }
    public string? ExchangeCode { get; set; }
}

public interface IExternalAuthService
{
    Task<ExternalLoginResult> ProcessAsync(
        string provider,
        string providerKey,
        string? email,
        bool emailVerified,
        string? firstName,
        string? lastName,
        string? displayName);
}