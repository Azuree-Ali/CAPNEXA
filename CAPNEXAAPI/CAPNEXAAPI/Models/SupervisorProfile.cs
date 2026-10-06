namespace CAPNEXAAPI.Models;

public class SupervisorProfile
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int MaxProjects { get; set; }
    public string? Department { get; set; }
    public string? ResearchInterests { get; set; }
    public string? Specialization { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
