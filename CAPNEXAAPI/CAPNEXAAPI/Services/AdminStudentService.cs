using CAPNEXAAPI.DTOs.Request;
using CAPNEXAAPI.DTOs.Response;
using CAPNEXAAPI.Models;
using CAPNEXAAPI.Repositories;
using CAPNEXAAPI.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CAPNEXAAPI.Services;

public sealed class AdminStudentService : IAdminStudentService
{
    public const int MaximumPageSize = 100;

    private const string LikeEscapeCharacter = "\\";

    private readonly UserManager<ApplicationUser> _users;
    private readonly IRepository<ApplicationUser> _userRepository;
    private readonly IRepository<IdentityUserRole<string>> _userRoles;
    private readonly IRepository<IdentityRole> _roles;
    private readonly IRefreshTokenService _refreshTokens;

    public AdminStudentService(
        UserManager<ApplicationUser> users,
        IRepository<ApplicationUser> userRepository,
        IRepository<IdentityUserRole<string>> userRoles,
        IRepository<IdentityRole> roles,
        IRefreshTokenService refreshTokens)
    {
        _users = users;
        _userRepository = userRepository;
        _userRoles = userRoles;
        _roles = roles;
        _refreshTokens = refreshTokens;
    }

    public async Task<AdminStudentResult<AdminStudentListDto>> GetStudentsAsync(AdminStudentQueryDto query)
    {
        if (query.Page < 1 || query.PageSize < 1 || query.PageSize > MaximumPageSize)
            return Failure<AdminStudentListDto>(
                AdminStudentError.InvalidRequest,
                "invalid_pagination");

        var studentRoleId = await _roles.GetQueryable()
            .Where(role => role.NormalizedName == CD.STUDENT_ROLE.ToUpperInvariant())
            .Select(role => role.Id)
            .FirstOrDefaultAsync();

        if (studentRoleId is null)
            return Success(EmptyPage(query));

        var studentUserIds = _userRoles.GetQueryable()
            .Where(link => link.RoleId == studentRoleId)
            .Select(link => link.UserId);

        var students = _userRepository.GetQueryable()
            .Where(user => studentUserIds.Contains(user.Id));

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = ContainsPattern(query.Search);
            students = students.Where(user =>
                EF.Functions.Like(user.FirstName, pattern, LikeEscapeCharacter) ||
                EF.Functions.Like(user.LastName, pattern, LikeEscapeCharacter) ||
                (user.Email != null &&
                 EF.Functions.Like(user.Email, pattern, LikeEscapeCharacter)) ||
                (user.StudentProfile != null &&
                 user.StudentProfile.StudentNumber != null &&
                 EF.Functions.Like(
                     user.StudentProfile.StudentNumber,
                     pattern,
                     LikeEscapeCharacter)));
        }

        if (!string.IsNullOrWhiteSpace(query.Department))
        {
            var department = query.Department.Trim();
            students = students.Where(user =>
                user.StudentProfile != null &&
                user.StudentProfile.Department == department);
        }

        if (!string.IsNullOrWhiteSpace(query.AcademicYear))
        {
            var academicYear = query.AcademicYear.Trim();
            students = students.Where(user =>
                user.StudentProfile != null &&
                user.StudentProfile.AcademicYear == academicYear);
        }

        if (query.IsActive.HasValue)
            students = students.Where(user => user.IsActive == query.IsActive.Value);

        var totalCount = await students.CountAsync();

        var rows = await students
            .OrderByDescending(user => user.CreatedAt)
            .ThenBy(user => user.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(user => new
            {
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                StudentNumber = user.StudentProfile != null
                    ? user.StudentProfile.StudentNumber
                    : null,
                Department = user.StudentProfile != null
                    ? user.StudentProfile.Department
                    : null,
                AcademicYear = user.StudentProfile != null
                    ? user.StudentProfile.AcademicYear
                    : null,
                user.EmailConfirmed,
                user.IsActive,
                user.CreatedAt
            })
            .ToListAsync();

        return Success(new AdminStudentListDto
        {
            Items = rows.Select(row => new AdminStudentListItemDto
            {
                UserId = row.Id,
                Email = row.Email,
                FirstName = row.FirstName,
                LastName = row.LastName,
                StudentNumber = row.StudentNumber,
                Department = row.Department,
                AcademicYear = row.AcademicYear,
                AcademicProfileComplete =
                    !string.IsNullOrWhiteSpace(row.StudentNumber) &&
                    !string.IsNullOrWhiteSpace(row.Department) &&
                    !string.IsNullOrWhiteSpace(row.AcademicYear),
                EmailConfirmed = row.EmailConfirmed,
                IsActive = row.IsActive,
                CreatedAt = row.CreatedAt
            }).ToArray(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        });
    }

    public async Task<AdminStudentResult<AdminStudentStatusDto>> UpdateStatusAsync(string userId, bool? isActive)
    {
        if (isActive is null)
            return Failure<AdminStudentStatusDto>(
                AdminStudentError.InvalidRequest,
                "invalid_status_payload");

        if (string.IsNullOrWhiteSpace(userId))
            return Failure<AdminStudentStatusDto>(
                AdminStudentError.NotFound,
                "student_not_found");

        var user = await _users.FindByIdAsync(userId);
        if (user is null || !await _users.IsInRoleAsync(user, CD.STUDENT_ROLE))
            return Failure<AdminStudentStatusDto>(
                AdminStudentError.NotFound,
                "student_not_found");

        if (user.IsActive == isActive.Value)
            return Success(ToStatus(user));

        user.IsActive = isActive.Value;
        var updateResult = await _users.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return Failure<AdminStudentStatusDto>(
                AdminStudentError.OperationFailed,
                "student_status_update_failed");

        if (!user.IsActive)
            await _refreshTokens.RevokeAllAsync(user.Id);

        return Success(ToStatus(user));
    }

    private static string ContainsPattern(string value)
    {
        var escaped = value.Trim()
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_");
        return $"%{escaped}%";
    }

    private static AdminStudentListDto EmptyPage(AdminStudentQueryDto query)
        => new()
        {
            Items = [],
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = 0,
            TotalPages = 0
        };

    private static AdminStudentStatusDto ToStatus(ApplicationUser user)
        => new()
        {
            UserId = user.Id,
            IsActive = user.IsActive
        };

    private static AdminStudentResult<T> Success<T>(T value)
        => new(value);

    private static AdminStudentResult<T> Failure<T>(AdminStudentError error, string code)
        => new(default, error, code);
}
