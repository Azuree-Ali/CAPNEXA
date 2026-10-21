using System.ComponentModel.DataAnnotations;

namespace CAPNEXAAPI.DTOs.Request;

public sealed class UpdateStudentProfileDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string LastName { get; set; } = string.Empty;

    [Phone]
    [StringLength(32)]
    public string? PhoneNumber { get; set; }
}
