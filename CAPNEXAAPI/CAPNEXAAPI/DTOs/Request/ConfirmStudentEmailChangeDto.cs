using System.ComponentModel.DataAnnotations;

namespace CAPNEXAAPI.DTOs.Request;

public sealed class ConfirmStudentEmailChangeDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string NewEmail { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;
}
