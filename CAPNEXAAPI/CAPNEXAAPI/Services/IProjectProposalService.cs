using CAPNEXAAPI.DTOs.Request;
using CAPNEXAAPI.DTOs.Response;

namespace CAPNEXAAPI.Services;

public interface IProjectProposalService
{
    Task<ProjectProposalResult<IReadOnlyList<ProjectProposalDto>>> GetMineAsync(string userId);
    Task<ProjectProposalResult<ProjectProposalDto>> CreateAsync(string userId, CreateProjectProposalDto request);
    Task<ProjectProposalResult<ProjectProposalDto>> UpdateAsync(string userId, int projectId, CreateProjectProposalDto request);
    Task<ProjectProposalResult<ProjectProposalDto>> SubmitAsync(string userId, int projectId);
}

public enum ProjectProposalError
{
    None,
    ProfileRequired,
    AccountDisabled,
    TeamRequired,
    NotTeamLeader,
    ActiveProposalExists,
    ProposalNotFound,
    ProposalNotEditable,
    ProposalNotReady
}

public sealed record ProjectProposalResult<T>(T? Value, ProjectProposalError Error = ProjectProposalError.None)
{
    public bool Succeeded => Error == ProjectProposalError.None;
}
