namespace CAPNEXAAPI.DTOs.Request;

public sealed class AdminStudentQueryDto
{
    public string? Search { get; set; }
    public string? Department { get; set; }
    public string? AcademicYear { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
