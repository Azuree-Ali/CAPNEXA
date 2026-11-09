using CAPNEXAAPI.DTOs.Request;
using CAPNEXAAPI.Services;
using CAPNEXAAPI.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;

namespace CAPNEXAAPI.Areas.Identity.Controllers;

[Area(CD.IDENTITY_AREA)]
[ApiController]
[Route("api/[area]/[controller]")]
[EnableRateLimiting("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly IConfiguration _configuration;
    private readonly IAuthenticationSchemeProvider _schemes;

    public AuthController(
        IAuthService auth,
        IConfiguration configuration,
        IAuthenticationSchemeProvider schemes)
    {
        _auth = auth;
        _configuration = configuration;
        _schemes = schemes;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterDto request)
    {
        var result = await _auth.RegisterAsync(request, GetApiBaseUrl());
        if (!result.Succeeded)
            return result.Conflict
                ? Conflict(new { message = result.Error })
                : BadRequest(new { message = result.Error });

        return Ok(new
        {
            message = result.Message,
            emailConfirmationSent = result.EmailConfirmationSent
        });
    }

    [HttpPost("resend-confirmation")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendConfirmation(ResendConfirmationDto request)
    {
        var result = await _auth.ResendConfirmationAsync(request.Email, GetApiBaseUrl());
        if (result.ServiceUnavailable)
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = result.Error });

        return Ok(new { message = result.Message });
    }

    [HttpGet("confirm-email")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        var confirmed = !string.IsNullOrWhiteSpace(userId) &&
                        !string.IsNullOrWhiteSpace(token) &&
                        await _auth.ConfirmEmailAsync(userId, token);

        var loginUrl = _configuration["Frontend:EmailConfirmationUrl"]
            ?? $"{GetFrontendBaseUrl()}/login";
        loginUrl = QueryHelpers.AddQueryString(loginUrl, "confirmed", confirmed.ToString().ToLowerInvariant());
        return Redirect(loginUrl);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginDto request)
    {
        var result = await _auth.LoginAsync(request);
        if (!result.Succeeded)
            return result.Unauthorized
                ? Unauthorized(new { message = result.Error })
                : BadRequest(new { message = result.Error });

        return Ok(result.Value);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(RefreshTokenDto request)
    {
        var result = await _auth.RefreshAsync(request);
        return result.Succeeded
            ? Ok(result.Value)
            : Unauthorized(new { message = result.Error });
    }

    [HttpGet("google/login")]
    [AllowAnonymous]
    public Task<IActionResult> GoogleLogin() => StartProviderLogin("Google");

    [HttpGet("github/login")]
    [AllowAnonymous]
    public Task<IActionResult> GitHubLogin() => StartProviderLogin("GitHub");

    private async Task<IActionResult> StartProviderLogin(string provider)
    {
        if (await _schemes.GetSchemeAsync(provider) is null)
            return BadRequest(new { message = $"{provider} login is not configured." });

        return Challenge(provider);
    }

    private string GetApiBaseUrl()
        => _configuration["AppUrls:ApiBaseUrl"]?.TrimEnd('/')
           ?? $"{Request.Scheme}://{Request.Host}";

    private string GetFrontendBaseUrl()
        => _configuration["AppUrls:FrontendBaseUrl"]?.TrimEnd('/')
           ?? $"{Request.Scheme}://{Request.Host}";
}
