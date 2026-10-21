namespace CAPNEXAAPI.DTOs.Response;

public sealed class StudentProfileDto
{
    public string UserId { get; init; } = string.Empty;
    public string? Email { get; init; }
    public bool EmailConfirmed { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public bool PhoneNumberConfirmed { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool AcademicProfileComplete { get; init; }
    public string? StudentNumber { get; init; }
    public string? Department { get; init; }
    public string? AcademicYear { get; init; }
    public string? Level { get; init; }
    public IReadOnlyList<StudentTeamDto> Teams { get; init; } = [];
}

public sealed class StudentTeamDto
{
    public int TeamId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string TeamCode { get; init; } = string.Empty;
    public bool IsLeader { get; init; }
    public bool IsCreator { get; init; }
    public DateTime? JoinedAt { get; init; }
    public IReadOnlyList<StudentProjectDto> Projects { get; init; } = [];
}

public sealed class StudentProjectDto
{
    public int ProjectId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
