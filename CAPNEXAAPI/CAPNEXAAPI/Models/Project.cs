namespace CAPNEXAAPI.Models;

public class Project
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public int? SupervisorId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Team Team { get; set; } = null!;
    public SupervisorProfile? Supervisor { get; set; }
    public ICollection<Milestone> Milestones { get; set; } = new List<Milestone>();
}
