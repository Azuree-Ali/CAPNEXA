using System.Security.Claims;
using CAPNEXAAPI.DTOs.Request;
using CAPNEXAAPI.Services;
using CAPNEXAAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CAPNEXAAPI.Areas.Admin.Controllers;

[Area(CD.ADMIN_AREA)]
[ApiController]
[Authorize(Roles = CD.ADMIN_ROLE)]
[Route("api/[area]/[controller]")]
public sealed class ProjectProposalsController : ControllerBase
{
    private readonly IAdminProposalService _proposals;

    public ProjectProposalsController(IAdminProposalService proposals)
    {
        _proposals = proposals;
    }

    [HttpGet]
    public async Task<IActionResult> GetProposals([FromQuery] AdminProposalQueryDto query)
        => ToResponse(await _proposals.GetProposalsAsync(query));

    [HttpGet("{projectId:int}")]
    public async Task<IActionResult> GetProposal(int projectId)
        => ToResponse(await _proposals.GetProposalAsync(projectId));

    [HttpPut("{projectId:int}/review")]
    public async Task<IActionResult> ReviewProposal(
        int projectId,
        [FromBody] ReviewProjectProposalDto request)
        => ToResponse(await _proposals.ReviewProposalAsync(
            GetUserId(),
            projectId,
            request,
            HttpContext?.Connection?.RemoteIpAddress?.ToString()));

    private string GetUserId()
        => User?.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User?.FindFirstValue("sub")
           ?? string.Empty;

    private IActionResult ToResponse<T>(AdminProposalResult<T> result)
    {
        if (result.Succeeded)
            return Ok(result.Value);

        var code = result.ErrorCode ?? "admin_proposal_operation_failed";
        var message = code switch
        {
            "proposal_not_found" => "The proposal was not found.",
            "proposal_not_pending_review" => "Only a proposal pending review can be reviewed.",
            "invalid_status_filter" => "Status must be PendingReview, Approved, or Rejected.",
            "invalid_pagination" => "Page must be at least 1 and pageSize must be between 1 and 100.",
            "invalid_decision" => "Decision must be Approved or Rejected.",
            "rejection_reason_required" => "A rejection reason is required.",
            "invalid_review_payload" => "The review reason or admin notes are too long.",
            _ => "The proposal operation could not be completed."
        };

        var body = new { code, message };
        return result.Error switch
        {
            AdminProposalError.NotFound => NotFound(body),
            AdminProposalError.Conflict => Conflict(body),
            AdminProposalError.InvalidRequest => BadRequest(body),
            _ => StatusCode(StatusCodes.Status500InternalServerError, body)
        };
    }
}
