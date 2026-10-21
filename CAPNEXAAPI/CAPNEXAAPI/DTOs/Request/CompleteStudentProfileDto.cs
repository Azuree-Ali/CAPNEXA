using System.ComponentModel.DataAnnotations;

namespace CAPNEXAAPI.DTOs.Request;

public sealed class CompleteStudentProfileDto
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string StudentNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Department { get; set; } = string.Empty;

    [Required]
    [StringLength(20, MinimumLength = 1)]
    public string AcademicYear { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Level { get; set; }
}
