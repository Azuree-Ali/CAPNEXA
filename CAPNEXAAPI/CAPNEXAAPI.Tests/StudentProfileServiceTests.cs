using System.Net.Mail;
using System.Net;
using System.Text.RegularExpressions;
using CAPNEXAAPI.DataAccess;
using CAPNEXAAPI.DTOs.Request;
using CAPNEXAAPI.Models;
using CAPNEXAAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CAPNEXAAPI.Tests;

public sealed class StudentProfileServiceTests
{
    [Fact]
    public async Task GetOwnProfile_MapsUserAndAcademicInformation()
    {
        await using var fixture = await Fixture.CreateAsync();
        var user = await fixture.CreateStudentAsync();
        fixture.Db.StudentProfiles.Add(new StudentProfile { UserId = user.Id, StudentNumber = "S42", Department = "CS", AcademicYear = "2026", Level = "3" });
        await fixture.Db.SaveChangesAsync();

        var result = await fixture.Service.GetOwnProfileAsync(user.Id);

        Assert.True(result.Succeeded);
        Assert.Equal("S42", result.Value!.StudentNumber);
        Assert.Equal("CS", result.Value.Department);
        Assert.True(result.Value.AcademicProfileComplete);
    }

    [Fact]
    public async Task GetOwnProfile_RejectsInactiveUser()
    {
        await using var fixture = await Fixture.CreateAsync();
        var user = await fixture.CreateStudentAsync(active: false);

        var result = await fixture.Service.GetOwnProfileAsync(user.Id);

        Assert.Equal(ProfileOperationError.Forbidden, result.Error);
        Assert.Equal("account_disabled", result.ErrorCode);
    }

    [Fact]
    public async Task UpdateOwnProfile_ChangesOnlyEditableFieldsAndResetsPhoneConfirmation()
    {
        await using var fixture = await Fixture.CreateAsync();
        var user = await fixture.CreateStudentAsync();
        user.PhoneNumber = "+201111111111";
        user.PhoneNumberConfirmed = true;
        await fixture.Users.UpdateAsync(user);
        fixture.Db.StudentProfiles.Add(new StudentProfile { UserId = user.Id, StudentNumber = "LOCKED", Department = "CS", AcademicYear = "2026" });
        await fixture.Db.SaveChangesAsync();

        var result = await fixture.Service.UpdateOwnProfileAsync(user.Id,
            new UpdateStudentProfileDto { FirstName = " New ", LastName = " Name ", PhoneNumber = "+201222222222" });

        var saved = await fixture.Users.FindByIdAsync(user.Id);
        var academic = await fixture.Db.StudentProfiles.SingleAsync(item => item.UserId == user.Id);
        Assert.True(result.Succeeded);
        Assert.Equal("New", saved!.FirstName);
        Assert.Equal("+201222222222", saved.PhoneNumber);
        Assert.False(saved.PhoneNumberConfirmed);
        Assert.Equal("LOCKED", academic.StudentNumber);
        Assert.Equal("CS", academic.Department);
    }

    [Fact]
    public async Task CompleteOwnProfile_AllowsOneCompletionAndRejectsDuplicateStudentNumber()
    {
        await using var fixture = await Fixture.CreateAsync();
        var first = await fixture.CreateStudentAsync();
        var second = await fixture.CreateStudentAsync("second@example.com");
        var request = new CompleteStudentProfileDto { StudentNumber = " S100 ", Department = " CS ", AcademicYear = " 2026 ", Level = " 3 " };

        var completed = await fixture.Service.CompleteOwnProfileAsync(first.Id, request);
        var repeated = await fixture.Service.CompleteOwnProfileAsync(first.Id, request);
        var duplicate = await fixture.Service.CompleteOwnProfileAsync(second.Id,
            new CompleteStudentProfileDto { StudentNumber = "S100", Department = "Math", AcademicYear = "2026" });

        Assert.True(completed.Succeeded);
        Assert.Equal("S100", completed.Value!.StudentNumber);
        Assert.Equal(ProfileOperationError.Conflict, repeated.Error);
        Assert.Equal("academic_profile_already_completed", repeated.ErrorCode);
        Assert.Equal(ProfileOperationError.Conflict, duplicate.Error);
        Assert.Equal("student_number_already_in_use", duplicate.ErrorCode);
    }

    [Fact]
    public async Task ChangePassword_ChangesLocalPasswordAndRevokesRefreshTokens()
    {
        await using var fixture = await Fixture.CreateAsync();
        var user = await fixture.CreateStudentAsync();

        var result = await fixture.Service.ChangeOwnPasswordAsync(user.Id,
            new ChangeStudentPasswordDto { CurrentPassword = "CurrentPass1!", NewPassword = "NewPass2!", ConfirmPassword = "NewPass2!" });

        Assert.True(result.Succeeded);
        Assert.True((await fixture.Users.CheckPasswordAsync(user, "NewPass2!")));
        Assert.Equal(user.Id, fixture.RefreshTokens.RevokedUserId);
    }

    [Fact]
    public async Task ChangePassword_RejectsWrongCurrentPasswordWithoutRevokingTokens()
    {
        await using var fixture = await Fixture.CreateAsync();
        var user = await fixture.CreateStudentAsync();

        var result = await fixture.Service.ChangeOwnPasswordAsync(user.Id,
            new ChangeStudentPasswordDto { CurrentPassword = "WrongPass1!", NewPassword = "NewPass2!", ConfirmPassword = "NewPass2!" });

        Assert.Equal(ProfileOperationError.InvalidRequest, result.Error);
        Assert.Equal("current_password_incorrect", result.ErrorCode);
        Assert.Null(fixture.RefreshTokens.RevokedUserId);
    }

    [Fact]
    public async Task ChangePassword_AllowsConfirmedExternalAccountToAddLocalPassword()
    {
        await using var fixture = await Fixture.CreateAsync();
        var user = await fixture.CreateStudentAsync(withPassword: false, confirmed: true);

        var result = await fixture.Service.ChangeOwnPasswordAsync(user.Id,
            new ChangeStudentPasswordDto { NewPassword = "NewPass2!", ConfirmPassword = "NewPass2!" });

        Assert.True(result.Succeeded);
        Assert.True(await fixture.Users.CheckPasswordAsync(user, "NewPass2!"));
        Assert.Equal(user.Id, fixture.RefreshTokens.RevokedUserId);
    }

    [Fact]
    public async Task ChangePassword_RequiresConfirmedEmailForExternalAccount()
    {
        await using var fixture = await Fixture.CreateAsync();
        var user = await fixture.CreateStudentAsync(withPassword: false, confirmed: false);

        var result = await fixture.Service.ChangeOwnPasswordAsync(user.Id,
            new ChangeStudentPasswordDto { NewPassword = "NewPass2!", ConfirmPassword = "NewPass2!" });

        Assert.Equal(ProfileOperationError.Forbidden, result.Error);
        Assert.Equal("email_not_confirmed", result.ErrorCode);
        Assert.Null(fixture.RefreshTokens.RevokedUserId);
    }

    [Fact]
    public async Task EmailChange_SendsConfirmationAndChangesAddressOnlyAfterTokenConfirmation()
    {
        await using var fixture = await Fixture.CreateAsync();
        var user = await fixture.CreateStudentAsync();

        var requested = await fixture.Service.RequestEmailChangeAsync(user.Id, "new@example.com");
        Assert.True(requested.Succeeded);
        Assert.Equal("student@example.com", (await fixture.Users.FindByIdAsync(user.Id))!.Email);
        Assert.Equal("new@example.com", fixture.Email.LastRecipient);

        var url = WebUtility.HtmlDecode(Regex.Match(fixture.Email.LastBody!, "href=\"([^\"]+)\"").Groups[1].Value);
        var query = QueryHelpers.ParseQuery(new Uri(url).Query);
        var confirmed = await fixture.Service.ConfirmEmailChangeAsync(
            query["userId"]!, query["newEmail"]!, query["token"]!);

        Assert.True(confirmed.Succeeded);
        var updated = await fixture.Users.FindByIdAsync(user.Id);
        Assert.Equal("new@example.com", updated!.Email);
        Assert.Equal("new@example.com", updated.UserName);
        Assert.True(updated.EmailConfirmed);
    }

    [Fact]
    public async Task EmailChange_RejectsDuplicateAddressAndMapsMailFailure()
    {
        await using var fixture = await Fixture.CreateAsync();
        var first = await fixture.CreateStudentAsync();
        await fixture.CreateStudentAsync("taken@example.com");

        var duplicate = await fixture.Service.RequestEmailChangeAsync(first.Id, "taken@example.com");
        Assert.Equal(ProfileOperationError.Conflict, duplicate.Error);

        fixture.Email.ThrowSmtpException = true;
        var mailFailure = await fixture.Service.RequestEmailChangeAsync(first.Id, "another@example.com");
        Assert.Equal(ProfileOperationError.ServiceUnavailable, mailFailure.Error);
        Assert.Equal("email_service_unavailable", mailFailure.ErrorCode);
    }

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ServiceProvider _provider;

        public ApplicationDbContext Db { get; }
        public UserManager<ApplicationUser> Users { get; }
        public StudentProfileService Service { get; }
        public TestEmailSender Email { get; } = new();
        public TestRefreshTokenService RefreshTokens { get; } = new();

        private Fixture(SqliteConnection connection, ServiceProvider provider, ApplicationDbContext db)
        {
            _connection = connection;
            _provider = provider;
            Db = db;
            Users = provider.GetRequiredService<UserManager<ApplicationUser>>();
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppUrls:ApiBaseUrl"] = "https://localhost:7072"
            }).Build();
            Service = new StudentProfileService(Users, Db, Email, RefreshTokens, configuration);
        }

        public static async Task<Fixture> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDataProtection();
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection));
            services.AddIdentityCore<ApplicationUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            var provider = services.BuildServiceProvider();
            var db = provider.GetRequiredService<ApplicationDbContext>();
            await db.Database.EnsureCreatedAsync();
            return new Fixture(connection, provider, db);
        }

        public async Task<ApplicationUser> CreateStudentAsync(
            string email = "student@example.com", bool withPassword = true, bool confirmed = true, bool active = true)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = confirmed,
                FirstName = "Test",
                LastName = "Student",
                IsActive = active
            };
            var result = withPassword
                ? await Users.CreateAsync(user, "CurrentPass1!")
                : await Users.CreateAsync(user);
            Assert.True(result.Succeeded, string.Join("; ", result.Errors.Select(error => error.Description)));
            var roles = _provider.GetRequiredService<RoleManager<IdentityRole>>();
            if (!await roles.RoleExistsAsync("Student"))
                Assert.True((await roles.CreateAsync(new IdentityRole("Student"))).Succeeded);
            Assert.True((await Users.AddToRoleAsync(user, "Student")).Succeeded);
            return user;
        }

        public async ValueTask DisposeAsync()
        {
            await Db.DisposeAsync();
            await _provider.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }

    private sealed class TestEmailSender : IEmailSender
    {
        public string? LastRecipient { get; private set; }
        public string? LastBody { get; private set; }
        public bool ThrowSmtpException { get; set; }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (ThrowSmtpException)
                throw new SmtpException("Test SMTP failure");
            LastRecipient = email;
            LastBody = htmlMessage;
            return Task.CompletedTask;
        }
    }

    private sealed class TestRefreshTokenService : IRefreshTokenService
    {
        public string? RevokedUserId { get; private set; }
        public Task<string> IssueAsync(string userId, string? jwtId = null) => throw new NotSupportedException();
        public Task<(ApplicationUser User, string AccessToken, int ExpiresInSeconds, string RefreshToken)?> RefreshAsync(string rawToken) => throw new NotSupportedException();
        public Task<bool> RevokeAsync(string rawToken, string userId) => throw new NotSupportedException();
        public Task RevokeAllAsync(string userId) { RevokedUserId = userId; return Task.CompletedTask; }
    }
}
