namespace CAPNEXAAPI.Models;

public class Team
{
    public int Id { get; set; }
    public int? CreatedByStudentId { get; set; }
    public string TeamCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? AcademicYear { get; set; }
    public int MaxMembers { get; set; }

    public StudentProfile? CreatedByStudent { get; set; }
    public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
