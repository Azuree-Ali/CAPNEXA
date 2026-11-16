namespace CAPNEXAAPI.DTOs.Response;

public sealed class AdminSupervisorListDto
{
    public IReadOnlyList<AdminSupervisorListItemDto> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
}

public sealed class AdminSupervisorListItemDto
{
    public int SupervisorId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public bool IsActive { get; init; }
    public string? Department { get; init; }
    public string? Specialization { get; init; }
    public string? ResearchInterests { get; init; }
    public int MaxProjects { get; init; }
    public int AssignedProjectsCount { get; init; }
    public int AvailableCapacity { get; init; }
}
