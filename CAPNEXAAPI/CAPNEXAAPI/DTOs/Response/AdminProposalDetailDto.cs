namespace CAPNEXAAPI.DTOs.Response;

public sealed class AdminProposalDetailDto
{
    public int ProjectId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public AdminProposalTeamDto Team { get; init; } = new();
    public AdminProposalReviewDto? Review { get; init; }
}

public sealed class AdminProposalTeamDto
{
    public int TeamId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string TeamCode { get; init; } = string.Empty;
    public int MaxMembers { get; init; }
    public int MemberCount { get; init; }
    public IReadOnlyList<AdminProposalMemberDto> Members { get; init; } = [];
}

public sealed class AdminProposalMemberDto
{
    public string UserId { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string StudentNumber { get; init; } = string.Empty;
    public string? Department { get; init; }
    public string? AcademicYear { get; init; }
    public bool IsLeader { get; init; }
    public DateTime JoinedAt { get; init; }
}

public sealed class AdminProposalReviewDto
{
    public string Decision { get; init; } = string.Empty;
    public string? Reason { get; init; }
    public string? AdminNotes { get; init; }
    public string? ReviewedByUserId { get; init; }
    public string? ReviewedByName { get; init; }
    public DateTime? ReviewedAt { get; init; }
}
