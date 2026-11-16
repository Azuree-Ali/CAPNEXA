using CAPNEXAAPI.DTOs.Request;
using CAPNEXAAPI.DTOs.Response;

namespace CAPNEXAAPI.Services;

public enum AdminProposalError
{
    None,
    NotFound,
    InvalidRequest,
    Conflict,
    OperationFailed
}

public sealed record AdminProposalResult<T>(
    T? Value,
    AdminProposalError Error = AdminProposalError.None,
    string? ErrorCode = null)
{
    public bool Succeeded => Error == AdminProposalError.None;
}

public interface IAdminProposalService
{
    Task<AdminProposalResult<AdminProposalListDto>> GetProposalsAsync(AdminProposalQueryDto query);
    Task<AdminProposalResult<AdminProposalDetailDto>> GetProposalAsync(int projectId);
    Task<AdminProposalResult<AdminProposalDetailDto>> ReviewProposalAsync(
        string adminUserId,
        int projectId,
        ReviewProjectProposalDto request,
        string? ipAddress);
}
