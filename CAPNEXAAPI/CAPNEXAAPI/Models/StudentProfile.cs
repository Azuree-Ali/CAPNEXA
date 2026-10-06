namespace CAPNEXAAPI.Models;

public class StudentProfile
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public string? AcademicYear { get; set; }
    public string? Level { get; set; }
    public string? Department { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public ICollection<TeamMember> TeamMemberships { get; set; } = new List<TeamMember>();
    public ICollection<Team> CreatedTeams { get; set; } = new List<Team>();
}
