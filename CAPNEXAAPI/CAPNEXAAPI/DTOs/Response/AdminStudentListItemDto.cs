namespace CAPNEXAAPI.DTOs.Response;

public sealed class AdminStudentListItemDto
{
    public string UserId { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? StudentNumber { get; init; }
    public string? Department { get; init; }
    public string? AcademicYear { get; init; }
    public bool AcademicProfileComplete { get; init; }
    public bool EmailConfirmed { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}
