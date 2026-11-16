using CAPNEXAAPI.DTOs.Request;
using CAPNEXAAPI.DTOs.Response;

namespace CAPNEXAAPI.Services;

public enum AdminSupervisorError
{
    None,
    NotFound,
    InvalidRequest,
    Conflict,
    OperationFailed
}

public sealed record AdminSupervisorResult<T>(
    T? Value,
    AdminSupervisorError Error = AdminSupervisorError.None,
    string? ErrorCode = null)
{
    public bool Succeeded => Error == AdminSupervisorError.None;
}

public interface IAdminSupervisorService
{
    Task<AdminSupervisorResult<AdminSupervisorListDto>> GetSupervisorsAsync(
        AdminSupervisorQueryDto query);

    Task<AdminSupervisorResult<AdminSupervisorDetailDto>> GetSupervisorAsync(int supervisorId);

    Task<AdminSupervisorResult<AdminSupervisorDetailDto>> UpdateMaxProjectsAsync(
        string adminUserId,
        int supervisorId,
        UpdateSupervisorCapacityDto? request,
        string? ipAddress);
}
