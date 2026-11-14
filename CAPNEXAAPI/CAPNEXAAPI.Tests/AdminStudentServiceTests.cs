using CAPNEXAAPI.DataAccess;
using CAPNEXAAPI.DTOs.Request;
using CAPNEXAAPI.DTOs.Response;
using CAPNEXAAPI.Models;
using CAPNEXAAPI.Repositories;
using CAPNEXAAPI.Services;
using CAPNEXAAPI.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace CAPNEXAAPI.Tests;

public sealed class AdminStudentServiceTests
{
    [Fact]
    public async Task List_ReturnsOnlyStudentAccounts_AndExposesOnlyDocumentedFields()
    {
        await using var fixture = await Fixture.CreateAsync();
        var student = await fixture.CreateStudentAsync("student@example.com");
        await fixture.CreateUserAsync("admin@example.com", CD.ADMIN_ROLE);
        await fixture.CreateUserAsync("supervisor@example.com", CD.SUPERVISOR_ROLE);

        var result = await fixture.Service.GetStudentsAsync(new AdminStudentQueryDto());

        Assert.True(result.Succeeded);
        var item = Assert.Single(result.Value!.Items);
        Assert.Equal(student.Id, item.UserId);
        Assert.Equal(
            new[]
            {
                "AcademicProfileComplete", "AcademicYear", "CreatedAt", "Department",
                "Email", "EmailConfirmed", "FirstName", "IsActive", "LastName",
                "StudentNumber", "UserId"
            },
            typeof(AdminStudentListItemDto).GetProperties()
                .Select(property => property.Name)
                .OrderBy(name => name)
                .ToArray());
    }

    [Theory]
    [InlineData("Amina")]
    [InlineData("Hassan")]
    [InlineData("amina@example.edu")]
    [InlineData("20260042")]
    public async Task List_SearchMatchesFirstNameLastNameEmailAndStudentNumber(string search)
    {
        await using var fixture = await Fixture.CreateAsync();
        var match = await fixture.CreateStudentAsync(
            "amina@example.edu", firstName: "Amina", lastName: "Hassan", studentNumber: "20260042");
        await fixture.CreateStudentAsync(
            "other@example.edu", firstName: "Other", lastName: "Person", studentNumber: "99999999");

        var result = await fixture.Service.GetStudentsAsync(
            new AdminStudentQueryDto { Search = search });

        Assert.True(result.Succeeded);
        var item = Assert.Single(result.Value!.Items);
        Assert.Equal(match.Id, item.UserId);
    }

    [Fact]
    public async Task List_SearchTreatsWildcardCharactersLiterally()
    {
        await using var fixture = await Fixture.CreateAsync();
        await fixture.CreateStudentAsync(
            "amina@example.edu", firstName: "Amina", lastName: "Hassan", studentNumber: "20260042");

        var wildcard = await fixture.Service.GetStudentsAsync(
            new AdminStudentQueryDto { Search = "%" });
        var underscore = await fixture.Service.GetStudentsAsync(
            new AdminStudentQueryDto { Search = "_" });

        Assert.True(wildcard.Succeeded);
        Assert.Empty(wildcard.Value!.Items);
        Assert.True(underscore.Succeeded);
        Assert.Empty(underscore.Value!.Items);
    }

    [Fact]
    public async Task List_AppliesDepartmentAcademicYearActiveAndCombinedFilters()
    {
        await using var fixture = await Fixture.CreateAsync();
        var amina = await fixture.CreateStudentAsync(
            "amina@example.edu", firstName: "Amina", studentNumber: "S1",
            department: "Computer Science", academicYear: "2026");
        var bilal = await fixture.CreateStudentAsync(
            "bilal@example.edu", firstName: "Bilal", studentNumber: "S2",
            department: "Computer Science", academicYear: "2025");
        var cara = await fixture.CreateStudentAsync(
            "cara@example.edu", firstName: "Cara", studentNumber: "S3",
            department: "Mathematics", academicYear: "2026", active: false);

        var department = await fixture.Service.GetStudentsAsync(
            new AdminStudentQueryDto { Department = "Computer Science" });
        var year = await fixture.Service.GetStudentsAsync(
            new AdminStudentQueryDto { AcademicYear = "2026" });
        var disabled = await fixture.Service.GetStudentsAsync(
            new AdminStudentQueryDto { IsActive = false });
        var combined = await fixture.Service.GetStudentsAsync(new AdminStudentQueryDto
        {
            Department = "Computer Science",
            AcademicYear = "2026",
            IsActive = true
        });

        Assert.Equal(
            new[] { amina.Id, bilal.Id }.OrderBy(id => id),
            department.Value!.Items.Select(item => item.UserId).OrderBy(id => id));
        Assert.Equal(
            new[] { amina.Id, cara.Id }.OrderBy(id => id),
            year.Value!.Items.Select(item => item.UserId).OrderBy(id => id));
        Assert.Equal(cara.Id, Assert.Single(disabled.Value!.Items).UserId);
        Assert.Equal(amina.Id, Assert.Single(combined.Value!.Items).UserId);
    }

    [Fact]
    public async Task List_UsesNewestFirstOrderingAndCountsBeforePagination()
    {
        await using var fixture = await Fixture.CreateAsync();
        var start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var created = new List<ApplicationUser>();
        for (var index = 0; index < 5; index++)
        {
            created.Add(await fixture.CreateStudentAsync(
                $"student{index}@example.com",
                firstName: $"Student{index}",
                createdAt: start.AddMinutes(index)));
        }

        var result = await fixture.Service.GetStudentsAsync(
            new AdminStudentQueryDto { Page = 2, PageSize = 2 });

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Value!.Page);
        Assert.Equal(2, result.Value.PageSize);
        Assert.Equal(5, result.Value.TotalCount);
        Assert.Equal(3, result.Value.TotalPages);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.Equal(created[2].Id, result.Value.Items[0].UserId);
        Assert.Equal(created[1].Id, result.Value.Items[1].UserId);
    }

    [Fact]
    public async Task List_UsesDefaultPageAndPageSize()
    {
        await using var fixture = await Fixture.CreateAsync();
        for (var index = 0; index < 21; index++)
            await fixture.CreateStudentAsync($"student{index}@example.com");

        var result = await fixture.Service.GetStudentsAsync(new AdminStudentQueryDto());

        Assert.True(result.Succeeded);
        Assert.Equal(1, result.Value!.Page);
        Assert.Equal(20, result.Value.PageSize);
        Assert.Equal(20, result.Value.Items.Count);
        Assert.Equal(21, result.Value.TotalCount);
        Assert.Equal(2, result.Value.TotalPages);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(-1, 20)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    [InlineData(1, 101)]
    public async Task List_RejectsInvalidPaginationValues(int page, int pageSize)
    {
        await using var fixture = await Fixture.CreateAsync();

        var result = await fixture.Service.GetStudentsAsync(
            new AdminStudentQueryDto { Page = page, PageSize = pageSize });

        Assert.Equal(AdminStudentError.InvalidRequest, result.Error);
        Assert.Equal("invalid_pagination", result.ErrorCode);
    }

    [Fact]
    public async Task List_ReturnsEmptyPageWhenStudentRoleDoesNotExist()
    {
        await using var fixture = await Fixture.CreateAsync();
        await fixture.CreateUserAsync("admin@example.com", CD.ADMIN_ROLE);

        var result = await fixture.Service.GetStudentsAsync(new AdminStudentQueryDto { Page = 2, PageSize = 10 });

        Assert.True(result.Succeeded);
        Assert.Empty(result.Value!.Items);
        Assert.Equal(2, result.Value.Page);
        Assert.Equal(10, result.Value.PageSize);
        Assert.Equal(0, result.Value.TotalCount);
        Assert.Equal(0, result.Value.TotalPages);
    }

    [Fact]
    public async Task Status_DeactivateRevokesOnlyThatStudentsTokens_AndActivateDoesNotRestoreThem()
    {
        await using var fixture = await Fixture.CreateAsync();
        var target = await fixture.CreateStudentAsync("target@example.com");
        var other = await fixture.CreateStudentAsync("other@example.com");
        await fixture.RefreshTokens.IssueAsync(target.Id);
        await fixture.RefreshTokens.IssueAsync(other.Id);

        var deactivated = await fixture.Service.UpdateStatusAsync(target.Id, false);
        var targetToken = await fixture.Db.RefreshTokens.AsNoTracking()
            .SingleAsync(token => token.UserId == target.Id);
        var otherToken = await fixture.Db.RefreshTokens.AsNoTracking()
            .SingleAsync(token => token.UserId == other.Id);

        Assert.True(deactivated.Succeeded);
        Assert.False(deactivated.Value!.IsActive);
        Assert.Equal(new[] { target.Id }, fixture.RefreshTokens.RevokedUserIds);
        Assert.NotNull(targetToken.RevokedAtUtc);
        Assert.Null(otherToken.RevokedAtUtc);

        var deactivatedAt = targetToken.RevokedAtUtc;
        var repeated = await fixture.Service.UpdateStatusAsync(target.Id, false);
        var repeatedToken = await fixture.Db.RefreshTokens.AsNoTracking()
            .SingleAsync(token => token.UserId == target.Id);

        Assert.True(repeated.Succeeded);
        Assert.Single(fixture.RefreshTokens.RevokedUserIds);
        Assert.Equal(deactivatedAt, repeatedToken.RevokedAtUtc);

        var activated = await fixture.Service.UpdateStatusAsync(target.Id, true);
        var reloaded = await fixture.Users.FindByIdAsync(target.Id);
        var activatedToken = await fixture.Db.RefreshTokens.AsNoTracking()
            .SingleAsync(token => token.UserId == target.Id);

        Assert.True(activated.Succeeded);
        Assert.True(activated.Value!.IsActive);
        Assert.True(reloaded!.IsActive);
        Assert.NotNull(activatedToken.RevokedAtUtc);
        Assert.Single(fixture.RefreshTokens.RevokedUserIds);
    }

    [Fact]
    public async Task Status_RepeatingCurrentStateSucceedsWithoutRevokingTokens()
    {
        await using var fixture = await Fixture.CreateAsync();
        var student = await fixture.CreateStudentAsync("active@example.com");
        await fixture.RefreshTokens.IssueAsync(student.Id);

        var result = await fixture.Service.UpdateStatusAsync(student.Id, true);
        var token = await fixture.Db.RefreshTokens.AsNoTracking()
            .SingleAsync(item => item.UserId == student.Id);

        Assert.True(result.Succeeded);
        Assert.True(result.Value!.IsActive);
        Assert.Empty(fixture.RefreshTokens.RevokedUserIds);
        Assert.Null(token.RevokedAtUtc);
    }

    [Fact]
    public async Task Status_ReturnsStudentNotFoundForUnknownAndNonStudentUsers()
    {
        await using var fixture = await Fixture.CreateAsync();
        var admin = await fixture.CreateUserAsync("admin@example.com", CD.ADMIN_ROLE);
        var supervisor = await fixture.CreateUserAsync("supervisor@example.com", CD.SUPERVISOR_ROLE);

        var missing = await fixture.Service.UpdateStatusAsync("missing-user", false);
        var adminResult = await fixture.Service.UpdateStatusAsync(admin.Id, false);
        var supervisorResult = await fixture.Service.UpdateStatusAsync(supervisor.Id, false);

        foreach (var result in new[] { missing, adminResult, supervisorResult })
        {
            Assert.Equal(AdminStudentError.NotFound, result.Error);
            Assert.Equal("student_not_found", result.ErrorCode);
        }

        Assert.True((await fixture.Users.FindByIdAsync(admin.Id))!.IsActive);
        Assert.True((await fixture.Users.FindByIdAsync(supervisor.Id))!.IsActive);
    }

    [Fact]
    public async Task Status_RejectsMissingPayloadValue()
    {
        await using var fixture = await Fixture.CreateAsync();
        var student = await fixture.CreateStudentAsync("student@example.com");

        var result = await fixture.Service.UpdateStatusAsync(student.Id, null);

        Assert.Equal(AdminStudentError.InvalidRequest, result.Error);
        Assert.Equal("invalid_status_payload", result.ErrorCode);
    }

    [Fact]
    public async Task Authentication_DisabledStudentCannotLogin()
    {
        await using var fixture = await Fixture.CreateAsync();
        var user = await fixture.CreateStudentAsync("disabled@example.com", withPassword: true);
        user.IsActive = false;
        await fixture.Users.UpdateAsync(user);

        var result = await fixture.Auth.LoginAsync(
            new LoginDto { Email = user.Email!, Password = "CurrentPass1!" });

        Assert.False(result.Succeeded);
        Assert.True(result.Unauthorized);
    }

    [Fact]
    public async Task Authentication_DisabledStudentCannotRefresh()
    {
        await using var fixture = await Fixture.CreateAsync();
        var user = await fixture.CreateStudentAsync("disabled-refresh@example.com", withPassword: true);
        var rawToken = await fixture.RefreshTokens.IssueAsync(user.Id);
        user.IsActive = false;
        await fixture.Users.UpdateAsync(user);

        var result = await fixture.Auth.RefreshAsync(new RefreshTokenDto { RefreshToken = rawToken });

        Assert.False(result.Succeeded);
        Assert.True(result.Unauthorized);
    }

    [Fact]
    public async Task Authentication_DisabledStudentCannotUseExternalOAuthExchange()
    {
        await using var fixture = await Fixture.CreateAsync();
        var user = await fixture.CreateStudentAsync("disabled-oauth@example.com");
        Assert.True((await fixture.Users.AddLoginAsync(
            user, new UserLoginInfo("Google", "google-key", "Google"))).Succeeded);
        user.IsActive = false;
        await fixture.Users.UpdateAsync(user);

        var result = await fixture.Auth.ExternalLoginAsync(
            "Google", "google-key", user.Email, true, user.FirstName, user.LastName, "External User");

        Assert.False(result.Succeeded);
        Assert.Equal("account_disabled", result.Error);
    }

    [Fact]
    public async Task Authentication_ActiveStudentCanLoginAndRefresh()
    {
        await using var fixture = await Fixture.CreateAsync();
        var user = await fixture.CreateStudentAsync("active-auth@example.com", withPassword: true);

        var login = await fixture.Auth.LoginAsync(
            new LoginDto { Email = user.Email!, Password = "CurrentPass1!" });
        var refresh = await fixture.Auth.RefreshAsync(
            new RefreshTokenDto { RefreshToken = login.Value!.RefreshToken });

        Assert.True(login.Succeeded);
        Assert.True(refresh.Succeeded);
        Assert.NotEqual(login.Value.RefreshToken, refresh.Value!.RefreshToken);
    }

    [Fact]
    public async Task Authentication_DisabledStudentCannotAccessProtectedStudentServices()
    {
        await using var fixture = await Fixture.CreateAsync();
        var user = await fixture.CreateStudentAsync("disabled-services@example.com", withPassword: true);
        user.IsActive = false;
        await fixture.Users.UpdateAsync(user);

        var profile = await fixture.Profiles.GetOwnProfileAsync(user.Id);
        var team = await fixture.Teams.GetMineAsync(user.Id);
        var proposals = await fixture.Proposals.GetMineAsync(user.Id);

        Assert.Equal(ProfileOperationError.Forbidden, profile.Error);
        Assert.Equal("account_disabled", profile.ErrorCode);
        Assert.Equal(TeamError.AccountDisabled, team.Error);
        Assert.Equal(ProjectProposalError.AccountDisabled, proposals.Error);
    }

    private sealed class Fixture : IAsyncDisposable
    {
        private static readonly JwtSettings JwtSettings = new()
        {
            Issuer = "CAPNEXAAPI.Tests",
            Audience = "CAPNEXA.Tests",
            Key = "test-signing-key-that-is-long-enough-for-hmac-sha256-0123456789",
            AccessTokenExpiresInMinutes = 15,
            RefreshTokenExpiresInDays = 7
        };

        private readonly SqliteConnection _connection;
        private readonly ServiceProvider _provider;

        public ApplicationDbContext Db { get; }
        public UserManager<ApplicationUser> Users { get; }
        public AdminStudentService Service { get; }
        public AuthService Auth { get; }
        public StudentProfileService Profiles { get; }
        public TeamService Teams { get; }
        public ProjectProposalService Proposals { get; }
        public RecordingRefreshTokenService RefreshTokens { get; }

        private Fixture(SqliteConnection connection, ServiceProvider provider, ApplicationDbContext db)
        {
            _connection = connection;
            _provider = provider;
            Db = db;
            Users = provider.GetRequiredService<UserManager<ApplicationUser>>();

            var tokenService = new TokenService(Users, Options.Create(JwtSettings));
            var refreshTokenRepository = new Repository<RefreshToken>(db);
            var realRefreshTokens = new RefreshTokenService(
                refreshTokenRepository, tokenService, Options.Create(JwtSettings));
            RefreshTokens = new RecordingRefreshTokenService(realRefreshTokens);

            var studentProfiles = new Repository<StudentProfile>(db);
            var teams = new Repository<Team>(db);
            var teamMembers = new Repository<TeamMember>(db);
            var projects = new Repository<Project>(db);
            var emailSender = new NoopEmailSender();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AppUrls:ApiBaseUrl"] = "https://localhost:7072"
                })
                .Build();

            Service = new AdminStudentService(
                Users,
                new Repository<ApplicationUser>(db),
                new Repository<IdentityUserRole<string>>(db),
                new Repository<IdentityRole>(db),
                RefreshTokens);
            Auth = new AuthService(
                Users, studentProfiles, emailSender, tokenService, realRefreshTokens);
            Profiles = new StudentProfileService(
                Users, studentProfiles, teams, teamMembers, projects, emailSender,
                realRefreshTokens, configuration);
            Teams = new TeamService(teams, teamMembers, studentProfiles);
            Proposals = new ProjectProposalService(projects, studentProfiles, teamMembers);
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
            string email,
            string firstName = "Test",
            string lastName = "Student",
            string? studentNumber = null,
            string department = "Computer Science",
            string academicYear = "2026",
            bool active = true,
            bool withPassword = false,
            bool withProfile = true,
            DateTime? createdAt = null)
            => await CreateUserAsync(
                email,
                CD.STUDENT_ROLE,
                active,
                firstName,
                lastName,
                createdAt,
                withProfile,
                studentNumber ?? $"S-{email}",
                department,
                academicYear,
                withPassword);

        public async Task<ApplicationUser> CreateUserAsync(
            string email,
            string role,
            bool active = true,
            string firstName = "Test",
            string lastName = "User",
            DateTime? createdAt = null,
            bool withProfile = false,
            string? studentNumber = null,
            string department = "Computer Science",
            string academicYear = "2026",
            bool withPassword = false)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName,
                IsActive = active,
                CreatedAt = createdAt ?? DateTime.UtcNow
            };

            var createResult = withPassword
                ? await Users.CreateAsync(user, "CurrentPass1!")
                : await Users.CreateAsync(user);
            Assert.True(createResult.Succeeded,
                string.Join("; ", createResult.Errors.Select(error => error.Description)));

            await EnsureRoleAsync(role);
            Assert.True((await Users.AddToRoleAsync(user, role)).Succeeded);

            if (withProfile)
            {
                Db.StudentProfiles.Add(new StudentProfile
                {
                    UserId = user.Id,
                    StudentNumber = studentNumber ?? $"S-{email}",
                    Department = department,
                    AcademicYear = academicYear
                });
                await Db.SaveChangesAsync();
            }

            return user;
        }

        private async Task EnsureRoleAsync(string roleName)
        {
            var roles = _provider.GetRequiredService<RoleManager<IdentityRole>>();
            if (await roles.RoleExistsAsync(roleName))
                return;

            var result = await roles.CreateAsync(new IdentityRole(roleName));
            Assert.True(result.Succeeded,
                string.Join("; ", result.Errors.Select(error => error.Description)));
        }

        public async ValueTask DisposeAsync()
        {
            await Db.DisposeAsync();
            await _provider.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }

    private sealed class RecordingRefreshTokenService : IRefreshTokenService
    {
        private readonly IRefreshTokenService _inner;

        public RecordingRefreshTokenService(IRefreshTokenService inner) => _inner = inner;

        public List<string> RevokedUserIds { get; } = [];

        public Task<string> IssueAsync(string userId, string? jwtId = null)
            => _inner.IssueAsync(userId, jwtId);

        public Task<(ApplicationUser User, string AccessToken, int ExpiresInSeconds, string RefreshToken)?> RefreshAsync(string rawToken)
            => _inner.RefreshAsync(rawToken);

        public async Task RevokeAllAsync(string userId)
        {
            RevokedUserIds.Add(userId);
            await _inner.RevokeAllAsync(userId);
        }
    }

    private sealed class NoopEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
            => Task.CompletedTask;
    }
}
