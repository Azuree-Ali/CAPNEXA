using CAPNEXAAPI.DTOs.Request;
using CAPNEXAAPI.DTOs.Response;

namespace CAPNEXAAPI.Services;

public enum AdminProjectAssignmentError
{
    None,
    NotFound,
    InvalidRequest,
    Conflict,
    OperationFailed
}

public sealed record AdminProjectAssignmentResult<T>(
    T? Value,
    AdminProjectAssignmentError Error = AdminProjectAssignmentError.None,
    string? ErrorCode = null)
{
    public bool Succeeded => Error == AdminProjectAssignmentError.None;
}

public interface IAdminProjectAssignmentService
{
    Task<AdminProjectAssignmentResult<AdminUnassignedProjectListDto>> GetUnassignedProjectsAsync(
        AdminUnassignedProjectsQueryDto query);

    Task<AdminProjectAssignmentResult<AdminProjectAssignmentDto>> AssignSupervisorAsync(
        string adminUserId,
        int projectId,
        AssignSupervisorDto? request,
        string? ipAddress);
}
