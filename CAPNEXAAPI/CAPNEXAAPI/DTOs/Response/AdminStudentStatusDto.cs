namespace CAPNEXAAPI.DTOs.Response;

public sealed class AdminStudentStatusDto
{
    public string UserId { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}
