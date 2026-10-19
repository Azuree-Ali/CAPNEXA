using CAPNEXAAPI.DTOs.Request;
using CAPNEXAAPI.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace CAPNEXAAPI.Areas.Identity.Controllers;

[Area("Identity")]
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;
    private readonly IAuthenticationSchemeProvider _schemeProvider;

    public AuthController(
        IAuthService authService,
        IConfiguration configuration,
        IAuthenticationSchemeProvider schemeProvider)
    {
        _authService = authService;
        _configuration = configuration;
        _schemeProvider = schemeProvider;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto request)
    {
        var apiBaseUrl = _configuration["AppUrls:ApiBaseUrl"]?.TrimEnd('/')
            ?? $"{Request.Scheme}://{Request.Host}";
        var result = await _authService.RegisterAsync(request, apiBaseUrl);
        if (!result.Succeeded)
        {
            var response = new { message = result.Error };
            return result.Conflict ? Conflict(response) : BadRequest(response);
        }

        return Ok(new { message = result.Message });
    }

    [HttpGet("confirm-email")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        var loginUrl = GetFrontendUrl("/login");
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            return Redirect($"{loginUrl}?confirmed=false");

        var confirmed = await _authService.ConfirmEmailAsync(userId, token);
        return Redirect($"{loginUrl}?confirmed={confirmed.ToString().ToLowerInvariant()}");
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        var result = await _authService.LoginAsync(request);
        if (!result.Succeeded)
        {
            var message = result.Error!;
            return result.Unauthorized
                ? Unauthorized(new { message })
                : BadRequest(new { message });
        }

        return Ok(result.Value);
    }

    [HttpPost("exchange")]
    [AllowAnonymous]
    public async Task<IActionResult> Exchange([FromBody] ExchangeCodeDto request)
    {
        var result = await _authService.ExchangeAsync(request);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { message = result.Error });
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto request)
    {
        var result = await _authService.RefreshAsync(request);
        return result.Succeeded ? Ok(result.Value) : Unauthorized(new { message = result.Error });
    }

    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> Revoke([FromBody] RefreshTokenDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Unauthorized();

        return await _authService.RevokeAsync(request.RefreshToken, userId)
            ? NoContent()
            : BadRequest(new { message = "Refresh token not found or already revoked." });
    }

    [HttpGet("google/login")]
    [AllowAnonymous]
    public Task<IActionResult> GoogleLogin() => StartExternalLoginAsync("Google");

    [HttpGet("github/login")]
    [AllowAnonymous]
    public Task<IActionResult> GitHubLogin() => StartExternalLoginAsync("GitHub");

    private async Task<IActionResult> StartExternalLoginAsync(string scheme)
    {
        if (await _schemeProvider.GetSchemeAsync(scheme) is null)
            return BadRequest(new { message = $"{scheme} login is not configured." });

        return Challenge(scheme);
    }

    private string GetFrontendUrl(string path)
    {
        var baseUrl = _configuration["AppUrls:FrontendBaseUrl"]?.TrimEnd('/')
            ?? $"{Request.Scheme}://{Request.Host}";
        return $"{baseUrl}{path}";
    }
}
