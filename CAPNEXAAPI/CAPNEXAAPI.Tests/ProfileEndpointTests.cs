using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using CAPNEXAAPI.Areas.Admin.Controllers;
using CAPNEXAAPI.Areas.Student.Controllers;
using CAPNEXAAPI.DTOs.Request;
using CAPNEXAAPI.DTOs.Response;
using CAPNEXAAPI.Services;
using CAPNEXAAPI.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace CAPNEXAAPI.Tests;

public sealed class ProfileEndpointTests
{
    private const string UserId = "student-1";

    [Fact]
    public async Task GetProfile_ReturnsProfileForAuthenticatedStudent()
    {
        var service = new FakeStudentProfileService { OwnProfileResult = Ok(Profile()) };
        var controller = StudentController(service);

        var result = await controller.GetMyProfile();

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(UserId, service.LastUserId);
    }

    [Fact]
    public async Task UpdateProfile_PassesEditableFieldsToService()
    {
        var service = new FakeStudentProfileService { OwnProfileResult = Ok(Profile()) };
        var controller = StudentController(service);
        var request = new UpdateStudentProfileDto { FirstName = "New", LastName = "Name", PhoneNumber = "+201000000000" };

        var result = await controller.UpdateMyProfile(request);

        Assert.IsType<OkObjectResult>(result);
        Assert.Same(request, service.LastUpdateRequest);
    }

    [Fact]
    public async Task ChangePassword_ReturnsNoContentAndCallsService()
    {
        var service = new FakeStudentProfileService { PasswordResult = Ok(true) };
        var controller = StudentController(service);
        var request = new ChangeStudentPasswordDto { CurrentPassword = "OldPass1!", NewPassword = "NewPass2!", ConfirmPassword = "NewPass2!" };

        var result = await controller.ChangePassword(request);

        Assert.IsType<NoContentResult>(result);
        Assert.Same(request, service.LastPasswordRequest);
    }

    [Theory]
    [InlineData("current_password_required", 400)]
    [InlineData("current_password_incorrect", 400)]
    [InlineData("password_policy_failed", 400)]
    [InlineData("email_not_confirmed", 403)]
    public async Task ChangePassword_MapsBusinessErrors(string code, int expectedStatus)
    {
        var error = code == "email_not_confirmed" ? ProfileOperationError.Forbidden : ProfileOperationError.InvalidRequest;
        var service = new FakeStudentProfileService { PasswordResult = Fail<bool>(error, code) };
        var result = await StudentController(service).ChangePassword(new ChangeStudentPasswordDto());

        Assert.Equal(expectedStatus, StatusCodeOf(result));
    }

    [Fact]
    public async Task CompleteProfile_DelegatesAndReturnsUpdatedProfile()
    {
        var service = new FakeStudentProfileService { OwnProfileResult = Ok(Profile()) };
        var controller = StudentController(service);
        var request = new CompleteStudentProfileDto { StudentNumber = "S100", Department = "CS", AcademicYear = "2026", Level = "3" };

        var result = await controller.CompleteMyAcademicProfile(request);

        Assert.IsType<OkObjectResult>(result);
        Assert.Same(request, service.LastCompleteRequest);
    }

    [Fact]
    public async Task RequestEmailChange_ReturnsConfirmationSentResponse()
    {
        var service = new FakeStudentProfileService { EmailRequestResult = Ok(true) };
        var result = await StudentController(service).RequestEmailChange(new ChangeStudentEmailDto { NewEmail = "new@example.com" });

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Contains("email_change_confirmation_sent", ok.Value!.ToString());
        Assert.Equal("new@example.com", service.LastEmail);
    }

    [Fact]
    public async Task ConfirmationLink_RendersEncodedValuesAndNoStoreHeaders()
    {
        var controller = StudentController(new FakeStudentProfileService());

        var result = controller.ShowEmailChangeConfirmation("id", "<script>alert(1)</script>@example.com", "token&x");

        var html = Assert.IsType<ContentResult>(result);
        Assert.Contains("&lt;script&gt;", html.Content);
        Assert.DoesNotContain("<script>", html.Content);
        Assert.Equal("no-store", controller.Response.Headers.CacheControl);
        Assert.Equal("no-referrer", controller.Response.Headers["Referrer-Policy"]);
    }

    [Fact]
    public async Task ConfirmEmailChange_DelegatesCapabilityAndReturnsSuccess()
    {
        var service = new FakeStudentProfileService { EmailConfirmResult = Ok(true) };
        var request = new ConfirmStudentEmailChangeDto { UserId = UserId, NewEmail = "new@example.com", Token = "token" };

        var result = await StudentController(service).ConfirmEmailChange(request);

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal((UserId, "new@example.com", "token"), service.LastConfirmation);
    }

    [Fact]
    public async Task AdminEndpoint_DelegatesStudentProfileLookup()
    {
        var service = new FakeStudentProfileService { AdminProfileResult = Ok(Profile()) };
        var result = await new StudentsController(service).GetStudentProfile(UserId);

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(UserId, service.LastAdminUserId);
    }

    [Fact]
    public void Controllers_DeclareExpectedAreaRoleAndRoutes()
    {
        var student = typeof(ProfileController);
        var admin = typeof(StudentsController);

        Assert.Equal(CD.STUDENT_AREA, student.GetCustomAttributes(typeof(AreaAttribute), false).Cast<AreaAttribute>().Single().RouteValue);
        Assert.Equal(CD.STUDENT_ROLE, student.GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>().Single().Roles);
        Assert.Equal("api/[area]/[controller]", student.GetCustomAttributes(typeof(RouteAttribute), false).Cast<RouteAttribute>().Single().Template);
        Assert.Equal(CD.ADMIN_ROLE, admin.GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>().Single().Roles);
    }

    [Fact]
    public void RequestDtos_RejectMissingOrMismatchedRequiredValues()
    {
        Assert.NotEmpty(Validate(new ChangeStudentPasswordDto { NewPassword = "password", ConfirmPassword = "different" }));
        Assert.NotEmpty(Validate(new CompleteStudentProfileDto()));
        Assert.NotEmpty(Validate(new ChangeStudentEmailDto { NewEmail = "not-an-email" }));
        Assert.NotEmpty(Validate(new ConfirmStudentEmailChangeDto { UserId = "id", NewEmail = "not-an-email", Token = "token" }));
        Assert.Empty(Validate(new UpdateStudentProfileDto { FirstName = "Ali", LastName = "Hassan", PhoneNumber = "+201000000000" }));
    }

    private static List<ValidationResult> Validate(object instance)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(instance, new ValidationContext(instance), results, validateAllProperties: true);
        return results;
    }

    private static int StatusCodeOf(IActionResult result)
        => result is ObjectResult objectResult ? objectResult.StatusCode!.Value : result is BadRequestResult ? 400 : 0;

    private static ProfileController StudentController(FakeStudentProfileService service)
    {
        var controller = new ProfileController(service)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, UserId) }, "test"))
                }
            }
        };
        return controller;
    }

    private static StudentProfileDto Profile() => new() { UserId = UserId, FirstName = "Ali", LastName = "Hassan", Teams = [] };
    private static ProfileOperationResult<T> Ok<T>(T value) => new(value);
    private static ProfileOperationResult<T> Fail<T>(ProfileOperationError error, string code) => new(default, error, code);

    private sealed class FakeStudentProfileService : IStudentProfileService
    {
        public ProfileOperationResult<StudentProfileDto> OwnProfileResult { get; init; } = Ok(Profile());
        public ProfileOperationResult<StudentProfileDto> AdminProfileResult { get; init; } = Ok(Profile());
        public ProfileOperationResult<bool> PasswordResult { get; init; } = Ok(true);
        public ProfileOperationResult<bool> EmailRequestResult { get; init; } = Ok(true);
        public ProfileOperationResult<bool> EmailConfirmResult { get; init; } = Ok(true);
        public string? LastUserId { get; private set; }
        public string? LastAdminUserId { get; private set; }
        public string? LastEmail { get; private set; }
        public (string UserId, string Email, string Token)? LastConfirmation { get; private set; }
        public UpdateStudentProfileDto? LastUpdateRequest { get; private set; }
        public ChangeStudentPasswordDto? LastPasswordRequest { get; private set; }
        public CompleteStudentProfileDto? LastCompleteRequest { get; private set; }

        public Task<ProfileOperationResult<StudentProfileDto>> GetOwnProfileAsync(string userId) { LastUserId = userId; return Task.FromResult(OwnProfileResult); }
        public Task<ProfileOperationResult<StudentProfileDto>> UpdateOwnProfileAsync(string userId, UpdateStudentProfileDto request) { LastUserId = userId; LastUpdateRequest = request; return Task.FromResult(OwnProfileResult); }
        public Task<ProfileOperationResult<bool>> ChangeOwnPasswordAsync(string userId, ChangeStudentPasswordDto request) { LastUserId = userId; LastPasswordRequest = request; return Task.FromResult(PasswordResult); }
        public Task<ProfileOperationResult<StudentProfileDto>> CompleteOwnProfileAsync(string userId, CompleteStudentProfileDto request) { LastUserId = userId; LastCompleteRequest = request; return Task.FromResult(OwnProfileResult); }
        public Task<ProfileOperationResult<bool>> RequestEmailChangeAsync(string userId, string newEmail) { LastUserId = userId; LastEmail = newEmail; return Task.FromResult(EmailRequestResult); }
        public Task<ProfileOperationResult<bool>> ConfirmEmailChangeAsync(string userId, string newEmail, string token) { LastConfirmation = (userId, newEmail, token); return Task.FromResult(EmailConfirmResult); }
        public Task<ProfileOperationResult<StudentProfileDto>> GetStudentProfileForAdminAsync(string userId) { LastAdminUserId = userId; return Task.FromResult(AdminProfileResult); }
    }
}
