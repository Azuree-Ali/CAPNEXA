using System.ComponentModel.DataAnnotations;

namespace CAPNEXAAPI.DTOs.Request;

public sealed class ChangeStudentPasswordDto
{
    [DataType(DataType.Password)]
    public string? CurrentPassword { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; } = string.Empty;
}
