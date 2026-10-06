namespace CAPNEXAAPI.Models;

public class AuditLog
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string? IpAddress { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Description { get; set; }
    public string? EntityName { get; set; }
    public string? EntityId { get; set; }

    public ApplicationUser? User { get; set; }
}
