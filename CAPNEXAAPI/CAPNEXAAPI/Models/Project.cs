namespace CAPNEXAAPI.Models;

public class Project
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public int? SupervisorId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = ProjectStatuses.Draft;
    public string? RejectionReason { get; set; }
    public string? ReviewReason { get; set; }
    public string? AdminNotes { get; set; }
    public string? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Team Team { get; set; } = null!;
    public SupervisorProfile? Supervisor { get; set; }
    public ApplicationUser? ReviewedByUser { get; set; }
    public ICollection<Milestone> Milestones { get; set; } = new List<Milestone>();
}

public static class ProjectStatuses
{
    public const string Draft = "Draft";
    public const string PendingReview = "PendingReview";
    public const string Rejected = "Rejected";
    public const string Approved = "Approved";
}
