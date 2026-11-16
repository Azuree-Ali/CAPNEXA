namespace CAPNEXAAPI.DTOs.Request;

public sealed class AdminSupervisorQueryDto
{
    public string? Search { get; set; }
    public string? Department { get; set; }
    public bool? IsActive { get; set; }
    public bool? HasCapacity { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
