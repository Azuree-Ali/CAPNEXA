using System.ComponentModel.DataAnnotations;

namespace CAPNEXAAPI.DTOs.Request;

public sealed class UpdateSupervisorCapacityDto
{
    [Required]
    [Range(0, 20)]
    public int? MaxProjects { get; set; }
}
