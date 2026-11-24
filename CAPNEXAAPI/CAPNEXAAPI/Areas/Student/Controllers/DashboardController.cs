using System.Security.Claims;
using CAPNEXAAPI.Services;
using CAPNEXAAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CAPNEXAAPI.Areas.Student.Controllers;

[Area(CD.STUDENT_AREA)]
[ApiController]
[Authorize(Roles = CD.STUDENT_ROLE)]
[Route("api/[area]/[controller]")]
public sealed class DashboardController : ControllerBase
{
    private readonly IStudentDashboardService _dashboard;

    public DashboardController(IStudentDashboardService dashboard)
    {
        _dashboard = dashboard;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboard()
        => ToResponse(await _dashboard.GetDashboardAsync(GetUserId()));

    private string GetUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;

    private IActionResult ToResponse<T>(StudentDashboardResult<T> result)
    {
        if (result.Succeeded)
            return Ok(result.Value);

        var (status, code, message) = result.Error switch
        {
            StudentDashboardError.ProfileRequired => (403, "profile_completion_required", "Complete your academic profile before using the dashboard."),
            StudentDashboardError.AccountDisabled => (403, "account_disabled", "This account has been disabled."),
            _ => (400, "dashboard_operation_failed", "The dashboard operation could not be completed.")
        };

        return StatusCode(status, new { code, message });
    }
}