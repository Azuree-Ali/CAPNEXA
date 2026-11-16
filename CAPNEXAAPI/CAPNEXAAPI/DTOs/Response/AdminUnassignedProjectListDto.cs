namespace CAPNEXAAPI.DTOs.Response;

public sealed class AdminUnassignedProjectListDto
{
    public IReadOnlyList<AdminUnassignedProjectListItemDto> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
}

public sealed class AdminUnassignedProjectListItemDto
{
    public int ProjectId { get; init; }
    public string Title { get; init; } = string.Empty;
    public DateTime? ReviewedAt { get; init; }
    public AdminProposalTeamSummaryDto Team { get; init; } = new();
    public string LeaderName { get; init; } = string.Empty;
}
