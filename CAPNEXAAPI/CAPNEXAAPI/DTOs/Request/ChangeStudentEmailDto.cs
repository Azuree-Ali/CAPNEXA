using System.ComponentModel.DataAnnotations;

namespace CAPNEXAAPI.DTOs.Request;

public sealed class ChangeStudentEmailDto
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string NewEmail { get; set; } = string.Empty;
}
