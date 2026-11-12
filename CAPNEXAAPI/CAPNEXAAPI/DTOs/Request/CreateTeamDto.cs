using System.ComponentModel.DataAnnotations;

namespace CAPNEXAAPI.DTOs.Request;

public sealed class CreateTeamDto
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
}
