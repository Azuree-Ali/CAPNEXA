using System.ComponentModel.DataAnnotations;

namespace CAPNEXAAPI.DTOs.Request;

public sealed class AssignSupervisorDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int? SupervisorId { get; set; }
}
