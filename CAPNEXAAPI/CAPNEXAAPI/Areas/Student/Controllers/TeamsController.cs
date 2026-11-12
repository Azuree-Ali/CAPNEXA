using System.Security.Claims;
using CAPNEXAAPI.DTOs.Request;
using CAPNEXAAPI.Services;
using CAPNEXAAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CAPNEXAAPI.Areas.Student.Controllers;

[Area(CD.STUDENT_AREA)]
[ApiController]
[Authorize(Roles = CD.STUDENT_ROLE)]
[Route("api/[area]/[controller]")]
public sealed class TeamsController : ControllerBase
{
    private readonly ITeamService _teams;

    public TeamsController(ITeamService teams)
    {
        _teams = teams;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTeamDto request)
    {
        var result = await _teams.CreateAsync(GetUserId(), request.Name);
        return result.Succeeded ? Ok(result.Value) : ToError(result.Error);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMine()
    {
        var result = await _teams.GetMineAsync(GetUserId());
        return result.Succeeded ? Ok(result.Value) : ToError(result.Error);
    }

    [HttpPost("join")]
    public async Task<IActionResult> Join([FromBody] JoinTeamDto request)
    {
        var result = await _teams.JoinAsync(GetUserId(), request.TeamCode);
        return result.Succeeded ? Ok(result.Value) : ToError(result.Error);
    }

    [HttpPost("leave")]
    public async Task<IActionResult> Leave()
    {
        var result = await _teams.LeaveAsync(GetUserId());
        return result.Succeeded ? NoContent() : ToError(result.Error);
    }

    private string GetUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;

    private IActionResult ToError(TeamError error)
    {
        var (status, code, message) = error switch
        {
            TeamError.ProfileRequired => (403, "profile_completion_required", "Complete your academic profile before using teams."),
            TeamError.AccountDisabled => (403, "account_disabled", "This account is disabled."),
            TeamError.StudentAlreadyInTeam => (409, "student_already_in_team", "You are already a member of a team."),
            TeamError.TeamNotFound => (404, "team_not_found", "No team was found for this code or account."),
            TeamError.TeamFull => (409, "team_full", "This team already has the maximum of five members."),
            TeamError.NotTeamLeader => (409, "team_leader_cannot_leave", "The team leader cannot leave the team."),
            _ => (400, "team_operation_failed", "The team operation could not be completed.")
        };

        return StatusCode(status, new { code, message });
    }
}
