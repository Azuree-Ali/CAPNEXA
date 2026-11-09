using CAPNEXAAPI.DTOs.Response;

namespace CAPNEXAAPI.Services;

public interface ITeamService
{
    Task<TeamResult<TeamDto>> CreateAsync(string userId, string name);
    Task<TeamResult<TeamDto>> GetMineAsync(string userId);
    Task<TeamResult<TeamDto>> JoinAsync(string userId, string teamCode);
    Task<TeamResult<bool>> LeaveAsync(string userId);
}
