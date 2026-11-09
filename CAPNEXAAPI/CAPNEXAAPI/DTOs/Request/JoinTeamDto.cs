using System.ComponentModel.DataAnnotations;

namespace CAPNEXAAPI.DTOs.Request;

public sealed class JoinTeamDto
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string TeamCode { get; set; } = string.Empty;
}
