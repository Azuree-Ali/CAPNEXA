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
public sealed class StudentsController : ControllerBase
{
    private readonly IStudentProfileService _profiles;
    private readonly IAdminStudentService _adminStudents;

    public StudentsController(
        IStudentProfileService profiles,
        IAdminStudentService adminStudents)
    {
        _profiles = profiles;
        _adminStudents = adminStudents;
    }

    [HttpGet]
    public async Task<IActionResult> GetStudents([FromQuery] AdminStudentQueryDto query)
    {
        var result = await _adminStudents.GetStudentsAsync(query);
        return ToResponse(result);
    }

    [HttpGet("{userId}/profile")]
    public async Task<IActionResult> GetStudentProfile(string userId)
    {
        var result = await _profiles.GetStudentProfileForAdminAsync(userId);
        if (result.Succeeded)
            return Ok(result.Value);

        return result.Error switch
        {
            ProfileOperationError.NotFound => NotFound(new
            {
                code = result.ErrorCode ?? "student_not_found",
                message = "The student was not found."
            }),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new
            {
                code = "profile_operation_failed",
                message = "The student profile could not be loaded."
            })
        };
    }

    [HttpPut("{userId}/status")]
    public async Task<IActionResult> UpdateStudentStatus(
        string userId,
        [FromBody] UpdateStudentStatusDto? request)
    {
        var result = await _adminStudents.UpdateStatusAsync(userId, request?.IsActive);
        return ToResponse(result);
    }

    private IActionResult ToResponse<T>(AdminStudentResult<T> result)
    {
        if (result.Succeeded)
            return Ok(result.Value);

        var code = result.ErrorCode ?? "admin_student_operation_failed";
        var message = code switch
        {
            "student_not_found" => "The student was not found.",
            "invalid_pagination" => "Page must be at least 1 and pageSize must be between 1 and 100.",
            "invalid_status_payload" => "The isActive value is required.",
            "student_status_update_failed" => "The student status could not be updated.",
            _ => "The admin student operation could not be completed."
        };

        var body = new { code, message };
        return result.Error switch
        {
            AdminStudentError.NotFound => NotFound(body),
            AdminStudentError.InvalidRequest => BadRequest(body),
            _ => StatusCode(StatusCodes.Status500InternalServerError, body)
        };
    }
}
