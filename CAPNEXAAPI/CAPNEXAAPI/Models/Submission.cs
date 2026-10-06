namespace CAPNEXAAPI.Models;

public class Submission
{
    public int Id { get; set; }
    public int MilestoneId { get; set; }
    public string Status { get; set; } = "Submitted";
    public string? FilePath { get; set; }
    public string? FileName { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public int Version { get; set; } = 1;

    public Milestone Milestone { get; set; } = null!;
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
