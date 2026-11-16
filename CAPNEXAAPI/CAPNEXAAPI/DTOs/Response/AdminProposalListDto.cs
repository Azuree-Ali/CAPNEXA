namespace CAPNEXAAPI.DTOs.Response;

public sealed class AdminProposalListDto
{
    public IReadOnlyList<AdminProposalListItemDto> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
}

public sealed class AdminProposalListItemDto
{
    public int ProjectId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime? SubmittedAt { get; init; }
    public DateTime? ReviewedAt { get; init; }
    public AdminProposalTeamSummaryDto Team { get; init; } = new();
    public string LeaderName { get; init; } = string.Empty;
}

public sealed class AdminProposalTeamSummaryDto
{
    public int TeamId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string TeamCode { get; init; } = string.Empty;
    public int MemberCount { get; init; }
}
