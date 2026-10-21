using System.Security.Claims;
using System.Text.Encodings.Web;
using CAPNEXAAPI.DTOs.Request;
using CAPNEXAAPI.Services;
using CAPNEXAAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CAPNEXAAPI.Areas.Student.Controllers;

[Area(CD.STUDENT_AREA)]
[ApiController]
[Authorize(Roles = CD.STUDENT_ROLE)]
[Route("api/[area]/[controller]")]
public sealed class ProfileController : ControllerBase
{
    private readonly IStudentProfileService _profiles;

    public ProfileController(IStudentProfileService profiles)
    {
        _profiles = profiles;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _profiles.GetOwnProfileAsync(userId);
        return result.Succeeded ? Ok(result.Value) : ToError(result.Error, result.ErrorCode);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateStudentProfileDto request)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _profiles.UpdateOwnProfileAsync(userId, request);
        return result.Succeeded ? Ok(result.Value) : ToError(result.Error, result.ErrorCode);
    }

    [HttpPost("password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangeStudentPasswordDto request)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _profiles.ChangeOwnPasswordAsync(userId, request);
        if (!result.Succeeded)
            return ToError(result.Error, result.ErrorCode);

        return NoContent();
    }

    [HttpPost("complete")]
    public async Task<IActionResult> CompleteMyAcademicProfile([FromBody] CompleteStudentProfileDto request)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _profiles.CompleteOwnProfileAsync(userId, request);
        return result.Succeeded ? Ok(result.Value) : ToError(result.Error, result.ErrorCode);
    }

    [HttpPost("email-change")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> RequestEmailChange([FromBody] ChangeStudentEmailDto request)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _profiles.RequestEmailChangeAsync(userId, request.NewEmail);
        if (!result.Succeeded)
            return ToError(result.Error, result.ErrorCode);

        return Ok(new
        {
            code = result.Value == true ? "email_change_confirmation_sent" : "email_unchanged",
            message = result.Value == true
                ? "A confirmation link was sent to the new email address."
                : "The requested email is already your current email."
        });
    }

    [HttpGet("confirm-email-change")]
    [AllowAnonymous]
    public IActionResult ShowEmailChangeConfirmation(
        [FromQuery] string userId,
        [FromQuery] string newEmail,
        [FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(userId) ||
            string.IsNullOrWhiteSpace(newEmail) ||
            string.IsNullOrWhiteSpace(token))
        {
            return BadRequest(new { code = "invalid_confirmation", message = "The confirmation link is invalid or incomplete." });
        }

        Response.Headers["Cache-Control"] = "no-store";
        Response.Headers["Referrer-Policy"] = "no-referrer";
        Response.Headers["Content-Security-Policy"] = "default-src 'none'; style-src 'unsafe-inline'; form-action 'self'; base-uri 'none'; frame-ancestors 'none'";

        var encoder = HtmlEncoder.Default;
        var html = $$"""
            <!doctype html>
            <html lang="en">
            <head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Confirm email change</title>
            <style>body{font:16px Arial,sans-serif;max-width:36rem;margin:10vh auto;padding:0 1rem;color:#172033}button{padding:.7rem 1rem;background:#2563eb;color:white;border:0;border-radius:.4rem;cursor:pointer}</style></head>
            <body><h1>Confirm email change</h1><p>Change your CAPNEXA sign-in email to <strong>{{encoder.Encode(newEmail)}}</strong>?</p>
            <form method="post" action="/api/Student/Profile/confirm-email-change">
              <input type="hidden" name="UserId" value="{{encoder.Encode(userId)}}">
              <input type="hidden" name="NewEmail" value="{{encoder.Encode(newEmail)}}">
              <input type="hidden" name="Token" value="{{encoder.Encode(token)}}">
              <button type="submit">Confirm email change</button>
            </form></body></html>
            """;
        return Content(html, "text/html; charset=utf-8");
    }

    [HttpPost("confirm-email-change")]
    [AllowAnonymous]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> ConfirmEmailChange([FromForm] ConfirmStudentEmailChangeDto request)
    {
        var result = await _profiles.ConfirmEmailChangeAsync(request.UserId, request.NewEmail, request.Token);
        if (!result.Succeeded)
            return ToError(result.Error, result.ErrorCode);

        return Ok(new { code = "email_change_confirmed", message = "Your email address was updated successfully." });
    }

    private string? GetCurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

    private IActionResult ToError(ProfileOperationError error, string? code)
    {
        var message = code switch
        {
            "account_disabled" => "This account is disabled.",
            "academic_profile_already_completed" => "The academic profile has already been completed.",
            "student_number_already_in_use" => "This student number is already in use.",
            "email_already_in_use" => "This email address is already in use.",
            "email_change_token_invalid_or_expired" => "The email confirmation link is invalid or has expired.",
            "email_change_failed" => "The email address could not be updated.",
            "email_service_unavailable" => "Email confirmation is temporarily unavailable. Please try again later.",
            "current_password_required" => "Enter your current password to change it.",
            "current_password_incorrect" => "The current password is incorrect.",
            "password_policy_failed" => "The new password does not meet the account password requirements.",
            "email_not_confirmed" => "Confirm your email before setting a password.",
            "student_not_found" => "The student was not found.",
            _ => "The requested profile operation could not be completed."
        };

        var body = new { code = code ?? "profile_operation_failed", message };
        return error switch
        {
            ProfileOperationError.NotFound => NotFound(body),
            ProfileOperationError.Forbidden => StatusCode(StatusCodes.Status403Forbidden, body),
            ProfileOperationError.Conflict => Conflict(body),
            ProfileOperationError.InvalidRequest => BadRequest(body),
            ProfileOperationError.ServiceUnavailable => StatusCode(StatusCodes.Status503ServiceUnavailable, body),
            _ => StatusCode(StatusCodes.Status500InternalServerError, body)
        };
    }
}
