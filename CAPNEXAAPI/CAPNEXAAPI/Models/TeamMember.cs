namespace CAPNEXAAPI.Models;

public class TeamMember
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public int StudentId { get; set; }
    public bool IsLeader { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public Team Team { get; set; } = null!;
    public StudentProfile Student { get; set; } = null!;
}
