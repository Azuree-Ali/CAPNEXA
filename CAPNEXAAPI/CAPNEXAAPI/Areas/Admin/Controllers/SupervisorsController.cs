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
public sealed class SupervisorsController : ControllerBase
{
    private readonly IAdminSupervisorService _supervisors;

    public SupervisorsController(IAdminSupervisorService supervisors)
    {
        _supervisors = supervisors;
    }

    [HttpGet]
    public async Task<IActionResult> GetSupervisors(
        [FromQuery] AdminSupervisorQueryDto query)
        => ToResponse(await _supervisors.GetSupervisorsAsync(query));

    [HttpGet("{supervisorId:int}")]
    public async Task<IActionResult> GetSupervisor(int supervisorId)
        => ToResponse(await _supervisors.GetSupervisorAsync(supervisorId));

    [HttpPut("{supervisorId:int}/max-projects")]
    public async Task<IActionResult> UpdateMaxProjects(
        int supervisorId,
        [FromBody] UpdateSupervisorCapacityDto? request)
        => ToResponse(await _supervisors.UpdateMaxProjectsAsync(
            GetUserId(),
            supervisorId,
            request,
            HttpContext?.Connection?.RemoteIpAddress?.ToString()));

    private string GetUserId()
        => User?.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User?.FindFirstValue("sub")
           ?? string.Empty;

    private IActionResult ToResponse<T>(AdminSupervisorResult<T> result)
    {
        if (result.Succeeded)
            return Ok(result.Value);

        var code = result.ErrorCode ?? "admin_supervisor_operation_failed";
        var message = code switch
        {
            "supervisor_not_found" => "The supervisor was not found.",
            "invalid_pagination" => "Page must be at least 1 and pageSize must be between 1 and 100.",
            "invalid_capacity_payload" => "maxProjects is required and must be between 0 and 20.",
            "capacity_below_assigned_projects" =>
                "maxProjects cannot be lower than the supervisor's assigned projects count.",
            _ => "The supervisor operation could not be completed."
        };

        var body = new { code, message };
        return result.Error switch
        {
            AdminSupervisorError.NotFound => NotFound(body),
            AdminSupervisorError.Conflict => Conflict(body),
            AdminSupervisorError.InvalidRequest => BadRequest(body),
            _ => StatusCode(StatusCodes.Status500InternalServerError, body)
        };
    }
}
