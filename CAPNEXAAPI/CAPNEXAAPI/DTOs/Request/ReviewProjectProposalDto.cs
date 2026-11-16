using System.ComponentModel.DataAnnotations;

namespace CAPNEXAAPI.DTOs.Request;

public sealed class ReviewProjectProposalDto
{
    [Required]
    [StringLength(50)]
    public string Decision { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Reason { get; set; }
}
