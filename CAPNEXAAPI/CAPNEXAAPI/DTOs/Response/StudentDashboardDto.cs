namespace CAPNEXAAPI.DTOs.Response;

public sealed class StudentDashboardDto
{
    public TeamDashboardDto? Team { get; init; }
    public ProposalDashboardDto? Proposal { get; init; }
    public ProjectDashboardDto? Project { get; init; }
    public IReadOnlyList<MilestoneDashboardDto> Milestones { get; init; } = [];
    public int ProgressPercentage { get; init; }
    public IReadOnlyList<UpcomingDeadlineDto> UpcomingDeadlines { get; init; } = [];
}

public sealed class TeamDashboardDto
{
    public int TeamId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string TeamCode { get; init; } = string.Empty;
    public int MaxMembers { get; init; }
    public int MemberCount { get; init; }
    public TeamLeaderDto Leader { get; init; } = null!;
    public IReadOnlyList<TeamMemberDashboardDto> Members { get; init; } = [];
}

public sealed class TeamLeaderDto
{
    public string UserId { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string StudentNumber { get; init; } = string.Empty;
}

public sealed class TeamMemberDashboardDto
{
    public string UserId { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string StudentNumber { get; init; } = string.Empty;
    public bool IsLeader { get; init; }
    public DateTime JoinedAt { get; init; }
}

public sealed class ProposalDashboardDto
{
    public int ProjectId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? RejectionReason { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? SubmittedAt { get; init; }
}

public sealed class ProjectDashboardDto
{
    public int ProjectId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public int? SupervisorId { get; init; }
    public string? SupervisorName { get; init; }
}

public sealed class MilestoneDashboardDto
{
    public int MilestoneId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime? DueDate { get; init; }
    public int Order { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public sealed class UpcomingDeadlineDto
{
    public int MilestoneId { get; init; }
    public string Title { get; init; } = string.Empty;
    public DateTime DueDate { get; init; }
    public int Order { get; init; }
}