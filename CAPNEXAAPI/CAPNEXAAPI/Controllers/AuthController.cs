using CAPNEXAAPI.DataAccess;
using CAPNEXAAPI.DTOs.Request;
using CAPNEXAAPI.DTOs.Response;
using CAPNEXAAPI.Models;
using CAPNEXAAPI.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;

namespace CAPNEXAAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;
    private readonly ITokenService _tokenService;
    private readonly RefreshTokenService _refreshTokenService;
    private readonly ExchangeCodeService _exchangeCodeService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        IEmailSender emailSender,
        IConfiguration configuration,
        ITokenService tokenService,
        RefreshTokenService refreshTokenService,
        ExchangeCodeService exchangeCodeService)
    {
        _userManager = userManager;
        _context = context;
        _emailSender = emailSender;
        _configuration = configuration;
        _tokenService = tokenService;
        _refreshTokenService = refreshTokenService;
        _exchangeCodeService = exchangeCodeService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (await _userManager.FindByEmailAsync(dto.Email) is not null)
            return BadRequest(new { message = "This email address is already in use." });

        if (await _context.StudentProfiles.AnyAsync(s => s.StudentNumber == dto.StudentNumber))
            return BadRequest(new { message = "This university ID is already in use." });

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var user = new ApplicationUser
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                UserName = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = "Registration failed.", errors = result.Errors.Select(e => e.Description) });
            }

            var studentProfile = new StudentProfile
            {
                UserId = user.Id,
                StudentNumber = dto.StudentNumber,
                Department = dto.Department,
                AcademicYear = dto.AcademicYear
            };

            _context.StudentProfiles.Add(studentProfile);
            await _context.SaveChangesAsync();

            await SendConfirmationEmailAsync(user);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            return BadRequest(new { message = "Registration failed. Please try again later." });
        }

        return Ok(new { message = "Registration completed successfully. Please check your email to activate your account." });
    }

    [HttpGet("confirm-email")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        var loginUrl = GetFrontendUrl("/login");

        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            return Redirect($"{loginUrl}?confirmed=false");

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Redirect($"{loginUrl}?confirmed=false");

        var result = await _userManager.ConfirmEmailAsync(user, token);
        return Redirect($"{loginUrl}?confirmed={result.Succeeded.ToString().ToLowerInvariant()}");
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized(new { message = "Invalid email or password." });

        if (!user.IsActive)
            return Unauthorized(new { message = "Your account has been disabled." });

        if (!await _userManager.IsEmailConfirmedAsync(user))
            return BadRequest(new { message = "Your account is not activated yet. Please activate your account first." });

        return Ok(await BuildAuthResponseAsync(user));
    }

    [HttpPost("exchange")]
    [AllowAnonymous]
    public async Task<IActionResult> Exchange([FromBody] ExchangeCodeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!_exchangeCodeService.TryConsume(dto.Code, out var userId))
            return BadRequest(new { message = "Invalid or expired authorization code." });

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return BadRequest(new { message = "Unable to sign in with this code." });

        return Ok(await BuildAuthResponseAsync(user));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _refreshTokenService.RefreshAsync(dto.RefreshToken);
        if (result is null)
            return Unauthorized(new { message = "Invalid, expired, or reused refresh token." });

        var response = new AuthResponseDto
        {
            AccessToken = result.Value.AccessToken,
            ExpiresInSeconds = result.Value.ExpiresInSeconds,
            RefreshToken = result.Value.RefreshToken,
            User = await BuildUserResponseAsync(result.Value.User)
        };

        return Ok(response);
    }

    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> Revoke([FromBody] RefreshTokenDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Unauthorized();

        var revoked = await _refreshTokenService.RevokeAsync(dto.RefreshToken, userId);
        return revoked
            ? NoContent()
            : BadRequest(new { message = "Refresh token not found or already revoked." });
    }

    [HttpGet("google/login")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleLogin()
    {
        if (!await IsProviderConfiguredAsync("Google"))
            return BadRequest(new { message = "Google login is not configured." });

        var properties = new AuthenticationProperties
        {
            RedirectUri = GetFrontendUrl(_configuration["Frontend:OAuthCallbackPath"] ?? "/oauth/callback")
        };

        return Challenge(properties, "Google");
    }

    [HttpGet("github/login")]
    [AllowAnonymous]
    public async Task<IActionResult> GitHubLogin()
    {
        if (!await IsProviderConfiguredAsync("GitHub"))
            return BadRequest(new { message = "GitHub login is not configured." });

        var properties = new AuthenticationProperties
        {
            RedirectUri = GetFrontendUrl(_configuration["Frontend:OAuthCallbackPath"] ?? "/oauth/callback")
        };

        return Challenge(properties, "GitHub");
    }

    private async Task<bool> IsProviderConfiguredAsync(string scheme)
    {
        var schemeProvider = HttpContext.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
        var registered = await schemeProvider.GetSchemeAsync(scheme);
        return registered is not null;
    }

    private async Task<AuthResponseDto> BuildAuthResponseAsync(ApplicationUser user)
    {
        var (accessToken, expiresInSeconds, jwtId) = await _tokenService.CreateAccessTokenAsync(user);
        var refreshToken = await _refreshTokenService.IssueAsync(user.Id, jwtId);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            ExpiresInSeconds = expiresInSeconds,
            RefreshToken = refreshToken,
            User = await BuildUserResponseAsync(user)
        };
    }

    private async Task<UserResponseDto> BuildUserResponseAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        return new UserResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles.ToList()
        };
    }

    private async Task SendConfirmationEmailAsync(ApplicationUser user)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        var apiBaseUrl = _configuration["AppUrls:ApiBaseUrl"]?.TrimEnd('/') ?? $"{Request.Scheme}://{Request.Host}";
        var confirmationLink = $"{apiBaseUrl}/api/Auth/confirm-email" +
                               $"?userId={WebUtility.UrlEncode(user.Id)}" +
                               $"&token={WebUtility.UrlEncode(token)}";

        var body = $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto">
              <h2>Welcome to CAPNEXA</h2>
              <p>Hi {WebUtility.HtmlEncode(user.FirstName)},</p>
              <p>Thanks for registering. Please confirm your email address to activate your account.</p>
              <p>
                <a href="{confirmationLink}" style="display:inline-block;padding:10px 20px;background:#2563eb;color:#fff;text-decoration:none;border-radius:6px">
                  Activate my account
                </a>
              </p>
              <p>If the button does not work, copy and paste this link into your browser:</p>
              <p><a href="{confirmationLink}">{confirmationLink}</a></p>
            </div>
            """;

        await _emailSender.SendEmailAsync(user.Email!, "CAPNEXA - Activate your account", body);
    }

    private string GetFrontendUrl(string path)
    {
        var baseUrl = _configuration["AppUrls:FrontendBaseUrl"]?.TrimEnd('/')
            ?? $"{Request.Scheme}://{Request.Host}";
        return $"{baseUrl}{path}";
    }
}