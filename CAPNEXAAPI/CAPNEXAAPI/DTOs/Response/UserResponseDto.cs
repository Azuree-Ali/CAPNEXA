namespace CAPNEXAAPI.DTOs.Response;

public class UserResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public ICollection<string> Roles { get; set; } = new List<string>();
}