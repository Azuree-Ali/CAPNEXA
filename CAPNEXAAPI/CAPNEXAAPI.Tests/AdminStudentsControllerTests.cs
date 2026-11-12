using System.Reflection;
using CAPNEXAAPI.Areas.Admin.Controllers;
using CAPNEXAAPI.DTOs.Request;
using CAPNEXAAPI.DTOs.Response;
using CAPNEXAAPI.Services;
using CAPNEXAAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CAPNEXAAPI.Tests;

public sealed class AdminStudentsControllerTests
{
    [Fact]
    public void Controller_RequiresAdminRoleAndDoesNotAllowAnonymousAccess()
    {
        var type = typeof(StudentsController);
        var authorize = Assert.Single(
            type.GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>());

        Assert.Equal(CD.ADMIN_ROLE, authorize.Roles);
        Assert.Empty(type.GetCustomAttributes(typeof(AllowAnonymousAttribute), false));

        foreach (var method in type.GetMethods(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            Assert.Empty(method.GetCustomAttributes(typeof(AllowAnonymousAttribute), false));
        }
    }

    [Fact]
    public async Task GetStudents_ReturnsOkAndDelegatesQueryToService()
    {
        var value = new AdminStudentListDto
        {
            Page = 2,
            PageSize = 5,
            TotalCount = 1,
            TotalPages = 1,
            Items = [new AdminStudentListItemDto { UserId = "student-1" }]
        };
        var admin = new FakeAdminStudentService
        {
            ListResult = new AdminStudentResult<AdminStudentListDto>(value)
        };
        var controller = Controller(admin);
        var query = new AdminStudentQueryDto { Page = 2, PageSize = 5 };

        var result = await controller.GetStudents(query);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(value, ok.Value);
        Assert.Same(query, admin.LastQuery);
    }

    [Fact]
    public async Task GetStudents_InvalidPagination_ReturnsBadRequestBusinessError()
    {
        var admin = new FakeAdminStudentService
        {
            ListResult = new AdminStudentResult<AdminStudentListDto>(
                default, AdminStudentError.InvalidRequest, "invalid_pagination")
        };
        var controller = Controller(admin);

        var result = await controller.GetStudents(new AdminStudentQueryDto { Page = 0 });

        Assert.Equal(400, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
        Assert.Contains("invalid_pagination", BodyOf(result));
    }

    [Fact]
    public async Task GetStudents_OperationFailure_ReturnsServerError()
    {
        var admin = new FakeAdminStudentService
        {
            ListResult = new AdminStudentResult<AdminStudentListDto>(
                default, AdminStudentError.OperationFailed, "admin_student_operation_failed")
        };
        var controller = Controller(admin);

        var result = await controller.GetStudents(new AdminStudentQueryDto());

        Assert.Equal(500, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_ReturnsOkAndDelegatesUserIdAndValue()
    {
        var value = new AdminStudentStatusDto { UserId = "student-1", IsActive = false };
        var admin = new FakeAdminStudentService
        {
            StatusResult = new AdminStudentResult<AdminStudentStatusDto>(value)
        };
        var controller = Controller(admin);

        var result = await controller.UpdateStudentStatus(
            "student-1", new UpdateStudentStatusDto { IsActive = false });

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(value, ok.Value);
        Assert.Equal(("student-1", (bool?)false), admin.LastStatus);
    }

    [Fact]
    public async Task UpdateStatus_UnknownStudent_ReturnsStudentNotFound()
    {
        var admin = new FakeAdminStudentService
        {
            StatusResult = new AdminStudentResult<AdminStudentStatusDto>(
                default, AdminStudentError.NotFound, "student_not_found")
        };
        var controller = Controller(admin);

        var result = await controller.UpdateStudentStatus(
            "missing", new UpdateStudentStatusDto { IsActive = false });

        Assert.Equal(404, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
        Assert.Contains("student_not_found", BodyOf(result));
    }

    [Fact]
    public async Task UpdateStatus_MissingValue_ReturnsBadRequestBusinessError()
    {
        var admin = new FakeAdminStudentService
        {
            StatusResult = new AdminStudentResult<AdminStudentStatusDto>(
                default, AdminStudentError.InvalidRequest, "invalid_status_payload")
        };
        var controller = Controller(admin);

        var result = await controller.UpdateStudentStatus(
            "student-1", new UpdateStudentStatusDto { IsActive = null });

        Assert.Equal(400, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
        Assert.Contains("invalid_status_payload", BodyOf(result));
    }

    [Fact]
    public async Task UpdateStatus_NullBody_ReturnsBadRequestBusinessError()
    {
        var admin = new FakeAdminStudentService
        {
            StatusResult = new AdminStudentResult<AdminStudentStatusDto>(
                default, AdminStudentError.InvalidRequest, "invalid_status_payload")
        };
        var controller = Controller(admin);

        var result = await controller.UpdateStudentStatus("student-1", null);

        Assert.Equal(400, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
        Assert.Contains("invalid_status_payload", BodyOf(result));
        Assert.Equal(("student-1", (bool?)null), admin.LastStatus);
    }

    [Fact]
    public async Task GetStudentProfile_ReturnsOkAndPreservesContract()
    {
        var profile = new StudentProfileDto
        {
            UserId = "student-1",
            FirstName = "Amina",
            LastName = "Hassan",
            AcademicProfileComplete = true,
            Teams =
            [
                new StudentTeamDto
                {
                    TeamId = 1,
                    Name = "Team Alpha",
                    TeamCode = "ABC123",
                    Projects =
                    [
                        new StudentProjectDto
                        {
                            ProjectId = 7,
                            Title = "Campus Helper",
                            Status = "Draft"
                        }
                    ]
                }
            ]
        };
        var profiles = new FakeStudentProfileService
        {
            AdminResult = new ProfileOperationResult<StudentProfileDto>(profile)
        };
        var controller = new StudentsController(profiles, new FakeAdminStudentService());

        var result = await controller.GetStudentProfile("student-1");

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(profile, ok.Value);
        Assert.Equal("student-1", profiles.LastAdminUserId);
    }

    [Fact]
    public async Task GetStudentProfile_UnknownOrNonStudent_ReturnsStudentNotFound()
    {
        var profiles = new FakeStudentProfileService
        {
            AdminResult = new ProfileOperationResult<StudentProfileDto>(
                default, ProfileOperationError.NotFound, "student_not_found")
        };
        var controller = new StudentsController(profiles, new FakeAdminStudentService());

        var result = await controller.GetStudentProfile("missing");

        Assert.Equal(404, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
        Assert.Contains("student_not_found", BodyOf(result));
    }

    [Fact]
    public async Task GetStudentProfile_UnexpectedFailure_ReturnsServerError()
    {
        var profiles = new FakeStudentProfileService
        {
            AdminResult = new ProfileOperationResult<StudentProfileDto>(
                default, ProfileOperationError.Conflict, "unexpected")
        };
        var controller = new StudentsController(profiles, new FakeAdminStudentService());

        var result = await controller.GetStudentProfile("student-1");

        Assert.Equal(500, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
        Assert.Contains("profile_operation_failed", BodyOf(result));
    }

    private static StudentsController Controller(FakeAdminStudentService admin)
        => new(new FakeStudentProfileService(), admin);

    private static string BodyOf(IActionResult result)
        => Assert.IsAssignableFrom<ObjectResult>(result).Value?.ToString() ?? string.Empty;

    private sealed class FakeAdminStudentService : IAdminStudentService
    {
        public AdminStudentResult<AdminStudentListDto> ListResult { get; init; }
            = new(new AdminStudentListDto());
        public AdminStudentResult<AdminStudentStatusDto> StatusResult { get; init; }
            = new(new AdminStudentStatusDto());
        public AdminStudentQueryDto? LastQuery { get; private set; }
        public (string UserId, bool? IsActive)? LastStatus { get; private set; }

        public Task<AdminStudentResult<AdminStudentListDto>> GetStudentsAsync(AdminStudentQueryDto query)
        {
            LastQuery = query;
            return Task.FromResult(ListResult);
        }

        public Task<AdminStudentResult<AdminStudentStatusDto>> UpdateStatusAsync(string userId, bool? isActive)
        {
            LastStatus = (userId, isActive);
            return Task.FromResult(StatusResult);
        }
    }

    private sealed class FakeStudentProfileService : IStudentProfileService
    {
        public ProfileOperationResult<StudentProfileDto> AdminResult { get; init; }
            = new(new StudentProfileDto());
        public string? LastAdminUserId { get; private set; }

        public Task<ProfileOperationResult<StudentProfileDto>> GetOwnProfileAsync(string userId)
            => throw new NotSupportedException();

        public Task<ProfileOperationResult<StudentProfileDto>> UpdateOwnProfileAsync(
            string userId, UpdateStudentProfileDto request)
            => throw new NotSupportedException();

        public Task<ProfileOperationResult<bool>> ChangeOwnPasswordAsync(
            string userId, ChangeStudentPasswordDto request)
            => throw new NotSupportedException();

        public Task<ProfileOperationResult<StudentProfileDto>> CompleteOwnProfileAsync(
            string userId, CompleteStudentProfileDto request)
            => throw new NotSupportedException();

        public Task<ProfileOperationResult<bool>> RequestEmailChangeAsync(string userId, string newEmail)
            => throw new NotSupportedException();

        public Task<ProfileOperationResult<bool>> ConfirmEmailChangeAsync(
            string userId, string newEmail, string token)
            => throw new NotSupportedException();

        public Task<ProfileOperationResult<StudentProfileDto>> GetStudentProfileForAdminAsync(string userId)
        {
            LastAdminUserId = userId;
            return Task.FromResult(AdminResult);
        }
    }
}
