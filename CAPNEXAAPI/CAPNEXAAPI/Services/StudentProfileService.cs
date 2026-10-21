using System.Net;
using System.Net.Mail;
using CAPNEXAAPI.DataAccess;
using CAPNEXAAPI.DTOs.Request;
using CAPNEXAAPI.DTOs.Response;
using CAPNEXAAPI.Models;
using CAPNEXAAPI.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace CAPNEXAAPI.Services;

public sealed class StudentProfileService : IStudentProfileService
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly ApplicationDbContext _db;
    private readonly IEmailSender _emailSender;
    private readonly IRefreshTokenService _refreshTokens;
    private readonly IConfiguration _configuration;

    public StudentProfileService(
        UserManager<ApplicationUser> users,
        ApplicationDbContext db,
        IEmailSender emailSender,
        IRefreshTokenService refreshTokens,
        IConfiguration configuration)
    {
        _users = users;
        _db = db;
        _emailSender = emailSender;
        _refreshTokens = refreshTokens;
        _configuration = configuration;
    }

    public async Task<ProfileOperationResult<StudentProfileDto>> GetOwnProfileAsync(string userId)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user is null)
            return Failure<StudentProfileDto>(ProfileOperationError.NotFound, "user_not_found");
        if (!user.IsActive)
            return Failure<StudentProfileDto>(ProfileOperationError.Forbidden, "account_disabled");

        return Success(await BuildProfileAsync(user));
    }

    public async Task<ProfileOperationResult<StudentProfileDto>> UpdateOwnProfileAsync(
        string userId,
        UpdateStudentProfileDto request)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user is null)
            return Failure<StudentProfileDto>(ProfileOperationError.NotFound, "user_not_found");
        if (!user.IsActive)
            return Failure<StudentProfileDto>(ProfileOperationError.Forbidden, "account_disabled");

        var firstName = request.FirstName.Trim();
        var lastName = request.LastName.Trim();
        var phoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber)
            ? null
            : request.PhoneNumber.Trim();

        var phoneChanged = !string.Equals(user.PhoneNumber, phoneNumber, StringComparison.Ordinal);
        user.FirstName = firstName;
        user.LastName = lastName;
        user.PhoneNumber = phoneNumber;
        if (phoneChanged)
            user.PhoneNumberConfirmed = false;

        var updateResult = await _users.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return Failure<StudentProfileDto>(ProfileOperationError.InvalidRequest, "profile_update_failed");

        return Success(await BuildProfileAsync(user));
    }

    public async Task<ProfileOperationResult<bool>> ChangeOwnPasswordAsync(
        string userId,
        ChangeStudentPasswordDto request)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user is null)
            return Failure<bool>(ProfileOperationError.NotFound, "user_not_found");
        if (!user.IsActive)
            return Failure<bool>(ProfileOperationError.Forbidden, "account_disabled");

        var hasPassword = await _users.HasPasswordAsync(user);
        if (hasPassword && string.IsNullOrWhiteSpace(request.CurrentPassword))
            return Failure<bool>(ProfileOperationError.InvalidRequest, "current_password_required");

        if (!hasPassword && !await _users.IsEmailConfirmedAsync(user))
            return Failure<bool>(ProfileOperationError.Forbidden, "email_not_confirmed");

        await using var transaction = await _db.Database.BeginTransactionAsync();
        var passwordResult = hasPassword
            ? await _users.ChangePasswordAsync(user, request.CurrentPassword!, request.NewPassword)
            : await _users.AddPasswordAsync(user, request.NewPassword);

        if (!passwordResult.Succeeded)
        {
            await transaction.RollbackAsync();
            var errorCode = passwordResult.Errors.Any(error => error.Code == "PasswordMismatch")
                ? "current_password_incorrect"
                : "password_policy_failed";
            return Failure<bool>(ProfileOperationError.InvalidRequest, errorCode);
        }

        await _refreshTokens.RevokeAllAsync(userId);
        await transaction.CommitAsync();
        return Success(true);
    }

    public async Task<ProfileOperationResult<StudentProfileDto>> CompleteOwnProfileAsync(
        string userId,
        CompleteStudentProfileDto request)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user is null)
            return Failure<StudentProfileDto>(ProfileOperationError.NotFound, "user_not_found");
        if (!user.IsActive)
            return Failure<StudentProfileDto>(ProfileOperationError.Forbidden, "account_disabled");

        if (await _db.StudentProfiles.AnyAsync(profile => profile.UserId == userId))
            return Failure<StudentProfileDto>(ProfileOperationError.Conflict, "academic_profile_already_completed");

        var studentNumber = request.StudentNumber.Trim();
        if (await _db.StudentProfiles.AnyAsync(profile => profile.StudentNumber == studentNumber))
            return Failure<StudentProfileDto>(ProfileOperationError.Conflict, "student_number_already_in_use");

        var profile = new StudentProfile
        {
            UserId = userId,
            StudentNumber = studentNumber,
            Department = request.Department.Trim(),
            AcademicYear = request.AcademicYear.Trim(),
            Level = string.IsNullOrWhiteSpace(request.Level) ? null : request.Level.Trim()
        };

        _db.StudentProfiles.Add(profile);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            _db.Entry(profile).State = EntityState.Detached;
            if (await _db.StudentProfiles.AnyAsync(item => item.StudentNumber == studentNumber))
                return Failure<StudentProfileDto>(ProfileOperationError.Conflict, "student_number_already_in_use");

            throw;
        }

        return Success(await BuildProfileAsync(user));
    }

    public async Task<ProfileOperationResult<bool>> RequestEmailChangeAsync(string userId, string newEmail)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user is null)
            return Failure<bool>(ProfileOperationError.NotFound, "user_not_found");
        if (!user.IsActive)
            return Failure<bool>(ProfileOperationError.Forbidden, "account_disabled");

        newEmail = newEmail.Trim();
        if (string.Equals(user.Email, newEmail, StringComparison.OrdinalIgnoreCase))
            return Success(false);

        var existingEmailUser = await _users.FindByEmailAsync(newEmail);
        if (existingEmailUser is not null && existingEmailUser.Id != user.Id)
            return Failure<bool>(ProfileOperationError.Conflict, "email_already_in_use");

        var existingNameUser = await _users.FindByNameAsync(newEmail);
        if (existingNameUser is not null && existingNameUser.Id != user.Id)
            return Failure<bool>(ProfileOperationError.Conflict, "email_already_in_use");

        var token = await _users.GenerateChangeEmailTokenAsync(user, newEmail);
        var apiBaseUrl = _configuration["AppUrls:ApiBaseUrl"]?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(apiBaseUrl) ||
            !Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out var apiUri) ||
            apiUri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException("AppUrls:ApiBaseUrl must be an absolute HTTPS URL.");
        }

        var confirmationUrl = QueryHelpers.AddQueryString(
            $"{apiBaseUrl}/api/Student/Profile/confirm-email-change",
            new Dictionary<string, string?>
            {
                ["userId"] = user.Id,
                ["newEmail"] = newEmail,
                ["token"] = token
            });

        var htmlMessage = $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto">
              <h2>Confirm your new email</h2>
              <p>Confirm the email address {WebUtility.HtmlEncode(newEmail)} for your CAPNEXA account.</p>
              <p><a href="{WebUtility.HtmlEncode(confirmationUrl)}">Confirm email address</a></p>
              <p>If you did not request this change, you can ignore this message.</p>
            </div>
            """;

        try
        {
            await _emailSender.SendEmailAsync(newEmail, "CAPNEXA - Confirm email change", htmlMessage);
        }
        catch (SmtpException)
        {
            return Failure<bool>(ProfileOperationError.ServiceUnavailable, "email_service_unavailable");
        }
        catch (InvalidOperationException)
        {
            return Failure<bool>(ProfileOperationError.ServiceUnavailable, "email_service_unavailable");
        }

        return Success(true);
    }

    public async Task<ProfileOperationResult<bool>> ConfirmEmailChangeAsync(
        string userId,
        string newEmail,
        string token)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user is null)
            return Failure<bool>(ProfileOperationError.NotFound, "user_not_found");
        if (!user.IsActive)
            return Failure<bool>(ProfileOperationError.Forbidden, "account_disabled");

        newEmail = newEmail.Trim();
        var existingEmailUser = await _users.FindByEmailAsync(newEmail);
        if (existingEmailUser is not null && existingEmailUser.Id != user.Id)
            return Failure<bool>(ProfileOperationError.Conflict, "email_already_in_use");

        var existingNameUser = await _users.FindByNameAsync(newEmail);
        if (existingNameUser is not null && existingNameUser.Id != user.Id)
            return Failure<bool>(ProfileOperationError.Conflict, "email_already_in_use");

        await using var transaction = await _db.Database.BeginTransactionAsync();
        var emailResult = await _users.ChangeEmailAsync(user, newEmail, token);
        if (!emailResult.Succeeded)
        {
            await transaction.RollbackAsync();
            return Failure<bool>(ProfileOperationError.InvalidRequest, "email_change_token_invalid_or_expired");
        }

        var usernameResult = await _users.SetUserNameAsync(user, newEmail);
        if (!usernameResult.Succeeded)
        {
            await transaction.RollbackAsync();
            if (usernameResult.Errors.Any(error => error.Code == "DuplicateUserName"))
                return Failure<bool>(ProfileOperationError.Conflict, "email_already_in_use");

            return Failure<bool>(ProfileOperationError.InvalidRequest, "email_change_failed");
        }

        await transaction.CommitAsync();
        return Success(true);
    }

    public async Task<ProfileOperationResult<StudentProfileDto>> GetStudentProfileForAdminAsync(string userId)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user is null || !await _users.IsInRoleAsync(user, CD.STUDENT_ROLE))
            return Failure<StudentProfileDto>(ProfileOperationError.NotFound, "student_not_found");

        return Success(await BuildProfileAsync(user));
    }

    private async Task<StudentProfileDto> BuildProfileAsync(ApplicationUser user)
    {
        var profile = await _db.StudentProfiles
            .AsNoTracking()
            .AsSplitQuery()
            .Include(item => item.TeamMemberships)
                .ThenInclude(membership => membership.Team)
                    .ThenInclude(team => team.Projects)
            .Include(item => item.CreatedTeams)
                .ThenInclude(team => team.Projects)
            .SingleOrDefaultAsync(item => item.UserId == user.Id);

        var teams = new Dictionary<int, StudentTeamDto>();
        if (profile is not null)
        {
            foreach (var membership in profile.TeamMemberships)
            {
                var team = membership.Team;
                teams[team.Id] = new StudentTeamDto
                {
                    TeamId = team.Id,
                    Name = team.Name,
                    TeamCode = team.TeamCode,
                    IsLeader = membership.IsLeader,
                    IsCreator = team.CreatedByStudentId == profile.Id,
                    JoinedAt = membership.JoinedAt,
                    Projects = team.Projects.Select(project => new StudentProjectDto
                    {
                        ProjectId = project.Id,
                        Title = project.Title,
                        Status = project.Status,
                        CreatedAt = project.CreatedAt
                    }).ToArray()
                };
            }

            foreach (var team in profile.CreatedTeams)
            {
                if (teams.ContainsKey(team.Id))
                    continue;

                teams[team.Id] = new StudentTeamDto
                {
                    TeamId = team.Id,
                    Name = team.Name,
                    TeamCode = team.TeamCode,
                    IsCreator = true,
                    Projects = team.Projects.Select(project => new StudentProjectDto
                    {
                        ProjectId = project.Id,
                        Title = project.Title,
                        Status = project.Status,
                        CreatedAt = project.CreatedAt
                    }).ToArray()
                };
            }
        }

        var academicProfileComplete = profile is not null &&
                                      !string.IsNullOrWhiteSpace(profile.StudentNumber) &&
                                      !string.IsNullOrWhiteSpace(profile.Department) &&
                                      !string.IsNullOrWhiteSpace(profile.AcademicYear);

        return new StudentProfileDto
        {
            UserId = user.Id,
            Email = user.Email,
            EmailConfirmed = user.EmailConfirmed,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            AcademicProfileComplete = academicProfileComplete,
            StudentNumber = profile?.StudentNumber,
            Department = profile?.Department,
            AcademicYear = profile?.AcademicYear,
            Level = profile?.Level,
            Teams = teams.Values.OrderBy(team => team.Name).ToArray()
        };
    }

    private static ProfileOperationResult<T> Success<T>(T value)
        => new(value);

    private static ProfileOperationResult<T> Failure<T>(ProfileOperationError error, string code)
        => new(default, error, code);
}
