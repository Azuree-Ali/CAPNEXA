namespace CAPNEXAAPI.Models;

public class Review
{
    public int Id { get; set; }
    public int SubmissionId { get; set; }
    public int? SupervisorId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public decimal? Grade { get; set; }
    public string? Feedback { get; set; }
    public string Status { get; set; } = "Pending";

    public Submission Submission { get; set; } = null!;
    public SupervisorProfile? Supervisor { get; set; }
}
