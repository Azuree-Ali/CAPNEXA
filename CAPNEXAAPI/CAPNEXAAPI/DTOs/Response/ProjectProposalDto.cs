namespace CAPNEXAAPI.DTOs.Response;

public sealed class ProjectProposalDto
{
    public int ProjectId { get; init; }
    public int TeamId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? RejectionReason { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? SubmittedAt { get; init; }
}
