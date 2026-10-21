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

    public StudentsController(IStudentProfileService profiles)
    {
        _profiles = profiles;
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
}
