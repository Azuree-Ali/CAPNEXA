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
public sealed class ProjectAssignmentsController : ControllerBase
{
    private readonly IAdminProjectAssignmentService _assignments;

    public ProjectAssignmentsController(IAdminProjectAssignmentService assignments)
    {
        _assignments = assignments;
    }

    [HttpGet("unassigned")]
    public async Task<IActionResult> GetUnassignedProjects(
        [FromQuery] AdminUnassignedProjectsQueryDto query)
        => ToResponse(await _assignments.GetUnassignedProjectsAsync(query));

    [HttpPut("{projectId:int}")]
    public async Task<IActionResult> AssignSupervisor(
        int projectId,
        [FromBody] AssignSupervisorDto? request)
        => ToResponse(await _assignments.AssignSupervisorAsync(
            GetUserId(),
            projectId,
            request,
            HttpContext?.Connection?.RemoteIpAddress?.ToString()));

    private string GetUserId()
        => User?.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User?.FindFirstValue("sub")
           ?? string.Empty;

    private IActionResult ToResponse<T>(AdminProjectAssignmentResult<T> result)
    {
        if (result.Succeeded)
            return Ok(result.Value);

        var code = result.ErrorCode ?? "admin_project_assignment_operation_failed";
        var message = code switch
        {
            "project_not_found" => "The project was not found.",
            "project_not_approved" => "Only approved projects can be assigned to a supervisor.",
            "supervisor_not_found" => "The supervisor was not found.",
            "supervisor_inactive" => "The supervisor account is inactive.",
            "supervisor_capacity_reached" =>
                "The supervisor has reached the maximum number of projects.",
            "invalid_pagination" => "Page must be at least 1 and pageSize must be between 1 and 100.",
            "invalid_supervisor_payload" => "supervisorId is required and must be at least 1.",
            _ => "The project assignment operation could not be completed."
        };

        var body = new { code, message };
        return result.Error switch
        {
            AdminProjectAssignmentError.NotFound => NotFound(body),
            AdminProjectAssignmentError.Conflict => Conflict(body),
            AdminProjectAssignmentError.InvalidRequest => BadRequest(body),
            _ => StatusCode(StatusCodes.Status500InternalServerError, body)
        };
    }
}
