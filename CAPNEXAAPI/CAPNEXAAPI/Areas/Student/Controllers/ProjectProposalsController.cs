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
public sealed class ProjectProposalsController : ControllerBase
{
    private readonly IProjectProposalService _proposals;

    public ProjectProposalsController(IProjectProposalService proposals)
    {
        _proposals = proposals;
    }

    [HttpGet]
    public async Task<IActionResult> GetMine()
        => ToResponse(await _proposals.GetMineAsync(GetUserId()));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectProposalDto request)
        => ToResponse(await _proposals.CreateAsync(GetUserId(), request));

    [HttpPut("{projectId:int}")]
    public async Task<IActionResult> Update(int projectId, [FromBody] CreateProjectProposalDto request)
        => ToResponse(await _proposals.UpdateAsync(GetUserId(), projectId, request));

    [HttpPost("{projectId:int}/submit")]
    public async Task<IActionResult> Submit(int projectId)
        => ToResponse(await _proposals.SubmitAsync(GetUserId(), projectId));

    private string GetUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;

    private IActionResult ToResponse<T>(ProjectProposalResult<T> result)
    {
        if (result.Succeeded)
            return Ok(result.Value);

        var (status, code, message) = result.Error switch
        {
            ProjectProposalError.ProfileRequired => (403, "profile_completion_required", "Complete your academic profile before using project proposals."),
            ProjectProposalError.AccountDisabled => (403, "account_disabled", "This account has been disabled."),
            ProjectProposalError.TeamRequired => (409, "team_required", "Join or create a team before using project proposals."),
            ProjectProposalError.NotTeamLeader => (403, "team_leader_required", "Only the team leader can submit the proposal for review."),
            ProjectProposalError.ActiveProposalExists => (409, "active_proposal_exists", "Your team already has a proposal that is not rejected."),
            ProjectProposalError.ProposalNotFound => (404, "proposal_not_found", "The proposal was not found for your team."),
            ProjectProposalError.ProposalNotEditable => (409, "proposal_not_editable", "Only a draft proposal can be edited or submitted."),
            ProjectProposalError.ProposalNotReady => (400, "proposal_not_ready", "Add a title and description before submitting the proposal."),
            _ => (400, "proposal_operation_failed", "The proposal operation could not be completed.")
        };

        return StatusCode(status, new { code, message });
    }
}
