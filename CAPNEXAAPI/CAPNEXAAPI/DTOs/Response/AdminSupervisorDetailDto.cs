namespace CAPNEXAAPI.DTOs.Response;

public sealed class AdminSupervisorDetailDto
{
    public int SupervisorId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public bool IsActive { get; init; }
    public string? Department { get; init; }
    public string? Specialization { get; init; }
    public string? ResearchInterests { get; init; }
    public int MaxProjects { get; init; }
    public int AssignedProjectsCount { get; init; }
    public int AvailableCapacity { get; init; }
    public IReadOnlyList<AdminSupervisorProjectDto> AssignedProjects { get; init; } = [];
}

public sealed class AdminSupervisorProjectDto
{
    public int ProjectId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int TeamId { get; init; }
    public string TeamName { get; init; } = string.Empty;
    public string TeamCode { get; init; } = string.Empty;
}
