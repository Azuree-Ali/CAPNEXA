using CAPNEXAAPI.DTOs.Request;
using CAPNEXAAPI.DTOs.Response;

namespace CAPNEXAAPI.Services;

public enum AdminStudentError
{
    None,
    NotFound,
    InvalidRequest,
    OperationFailed
}

public sealed record AdminStudentResult<T>(
    T? Value,
    AdminStudentError Error = AdminStudentError.None,
    string? ErrorCode = null)
{
    public bool Succeeded => Error == AdminStudentError.None;
}

public interface IAdminStudentService
{
    Task<AdminStudentResult<AdminStudentListDto>> GetStudentsAsync(AdminStudentQueryDto query);
    Task<AdminStudentResult<AdminStudentStatusDto>> UpdateStatusAsync(string userId, bool? isActive);
}
