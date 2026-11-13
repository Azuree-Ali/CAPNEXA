namespace CAPNEXAAPI.DTOs.Response;

public sealed class AdminStudentListDto
{
    public IReadOnlyList<AdminStudentListItemDto> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
}
