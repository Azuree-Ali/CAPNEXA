namespace CAPNEXAAPI.DTOs.Response;

public sealed class AdminProjectAssignmentDto
{
    public int ProjectId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public AdminProposalTeamSummaryDto Team { get; init; } = new();
    public AdminProjectSupervisorDto Supervisor { get; init; } = new();
}

public sealed class AdminProjectSupervisorDto
{
    public int SupervisorId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public int MaxProjects { get; init; }
    public int AssignedProjectsCount { get; init; }
    public int AvailableCapacity { get; init; }
}
