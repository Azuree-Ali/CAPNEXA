using CAPNEXAAPI.DataAccess;
using CAPNEXAAPI.DTOs.Request;
using CAPNEXAAPI.Models;
using CAPNEXAAPI.Repositories;
using CAPNEXAAPI.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CAPNEXAAPI.Tests;

public sealed class AdminProposalServiceTests
{
    [Fact]
    public async Task GetProposals_DefaultsToPendingReviewAndExcludesDrafts()
    {
        await using var fixture = await Fixture.CreateAsync();
        var pendingLeader = await fixture.CreateStudentAsync(
            "pending-leader@example.com", "Lina", "Leader", "S1");
        var pendingTeam = await fixture.CreateTeamAsync("Pending Team", "PEND0001", pendingLeader);
        var draftLeader = await fixture.CreateStudentAsync(
            "draft-leader@example.com", "Drew", "Draft", "S2");
        var draftTeam = await fixture.CreateTeamAsync("Draft Team", "DRFT0002", draftLeader);

        await fixture.CreateProposalAsync(
            pendingTeam, "Campus Navigator", ProjectStatuses.PendingReview, DateTime.UtcNow.AddDays(-1));
        await fixture.CreateProposalAsync(
            draftTeam, "Secret Draft", ProjectStatuses.Draft, null);

        var result = await fixture.Service.GetProposalsAsync(new AdminProposalQueryDto());

        Assert.True(result.Succeeded);
        var item = Assert.Single(result.Value!.Items);
        Assert.Equal("Campus Navigator", item.Title);
        Assert.Equal(ProjectStatuses.PendingReview, item.Status);
        Assert.Equal("Lina Leader", item.LeaderName);
        Assert.Equal("PEND0001", item.Team.TeamCode);
        Assert.Equal(1, item.Team.MemberCount);
        Assert.Equal(1, result.Value.TotalCount);
    }

    [Fact]
    public async Task GetProposals_FiltersByStatusAndSearchesTitleTeamNameAndTeamCode()
    {
        await using var fixture = await Fixture.CreateAsync();
        var rocketLeader = await fixture.CreateStudentAsync(
            "rocket-leader@example.com", "Rita", "Rocket", "S3");
        var rocketTeam = await fixture.CreateTeamAsync("Rocket Team", "RCK001", rocketLeader);
        var oceanLeader = await fixture.CreateStudentAsync(
            "ocean-leader@example.com", "Omar", "Ocean", "S4");
        var oceanTeam = await fixture.CreateTeamAsync("Ocean Team", "OCN002", oceanLeader);

        await fixture.CreateProposalAsync(
            rocketTeam, "Campus Navigator", ProjectStatuses.PendingReview, DateTime.UtcNow.AddDays(-2));
        await fixture.CreateProposalAsync(
            oceanTeam,
            "Ocean Tracker",
            ProjectStatuses.Approved,
            DateTime.UtcNow.AddDays(-3),
            DateTime.UtcNow.AddDays(-1));

        var approved = await fixture.Service.GetProposalsAsync(new AdminProposalQueryDto
        {
            Status = ProjectStatuses.Approved,
            Search = "ocean"
        });
        var pendingByTeamCode = await fixture.Service.GetProposalsAsync(new AdminProposalQueryDto
        {
            Search = "rck"
        });
        var pendingByTeamName = await fixture.Service.GetProposalsAsync(new AdminProposalQueryDto
        {
            Search = "rocket"
        });

        Assert.Equal("Ocean Tracker", Assert.Single(approved.Value!.Items).Title);
        Assert.Equal("Campus Navigator", Assert.Single(pendingByTeamCode.Value!.Items).Title);
        Assert.Equal("Campus Navigator", Assert.Single(pendingByTeamName.Value!.Items).Title);
    }

    [Fact]
    public async Task GetProposals_OrdersPendingOldestFirstAndDecidedNewestFirst()
    {
        await using var fixture = await Fixture.CreateAsync();
        var olderLeader = await fixture.CreateStudentAsync(
            "older-leader@example.com", "Olga", "Older", "S5");
        var newerLeader = await fixture.CreateStudentAsync(
            "newer-leader@example.com", "Nora", "Newer", "S6");
        var olderTeam = await fixture.CreateTeamAsync("Older Team", "OLD0001", olderLeader);
        var newerTeam = await fixture.CreateTeamAsync("Newer Team", "NEW0002", newerLeader);

        await fixture.CreateProposalAsync(
            newerTeam, "Newer Pending", ProjectStatuses.PendingReview, DateTime.UtcNow.AddDays(-1));
        await fixture.CreateProposalAsync(
            olderTeam, "Older Pending", ProjectStatuses.PendingReview, DateTime.UtcNow.AddDays(-5));

        var pending = await fixture.Service.GetProposalsAsync(new AdminProposalQueryDto());

        Assert.Equal(
            ["Older Pending", "Newer Pending"],
            pending.Value!.Items.Select(item => item.Title).ToArray());
    }

    [Fact]
    public async Task GetProposals_InvalidPaginationAndStatusReturnBusinessErrors()
    {
        await using var fixture = await Fixture.CreateAsync();

        var badPage = await fixture.Service.GetProposalsAsync(new AdminProposalQueryDto { Page = 0 });
        var badPageSize = await fixture.Service.GetProposalsAsync(
            new AdminProposalQueryDto { PageSize = AdminProposalService.MaximumPageSize + 1 });
        var badStatus = await fixture.Service.GetProposalsAsync(
            new AdminProposalQueryDto { Status = ProjectStatuses.Draft });

        Assert.Equal(AdminProposalError.InvalidRequest, badPage.Error);
        Assert.Equal("invalid_pagination", badPage.ErrorCode);
        Assert.Equal("invalid_pagination", badPageSize.ErrorCode);
        Assert.Equal("invalid_status_filter", badStatus.ErrorCode);
    }

    [Fact]
    public async Task GetProposal_ReturnsTeamMembersLeaderFirstAndNullReviewForPending()
    {
        await using var fixture = await Fixture.CreateAsync();
        var leader = await fixture.CreateStudentAsync(
            "detail-leader@example.com", "Lina", "Leader", "S7");
        var member = await fixture.CreateStudentAsync(
            "detail-member@example.com", "Mona", "Member", "S8");
        var team = await fixture.CreateTeamAsync("Detail Team", "DTL0001", leader, member);
        var project = await fixture.CreateProposalAsync(
            team, "Detail Proposal", ProjectStatuses.PendingReview, DateTime.UtcNow.AddDays(-1));

        var result = await fixture.Service.GetProposalAsync(project.Id);

        Assert.True(result.Succeeded);
        var detail = result.Value!;
        Assert.Null(detail.Review);
        Assert.Equal("Detail Team", detail.Team.Name);
        Assert.Equal(2, detail.Team.MemberCount);
        Assert.Equal(2, detail.Team.Members.Count);
        Assert.True(detail.Team.Members[0].IsLeader);
        Assert.Equal("Lina", detail.Team.Members[0].FirstName);
        Assert.Equal("detail-member@example.com", detail.Team.Members[1].Email);
        Assert.Equal("S8", detail.Team.Members[1].StudentNumber);
    }

    [Fact]
    public async Task GetProposal_ReturnsNotFoundForDraftOrMissing()
    {
        await using var fixture = await Fixture.CreateAsync();
        var leader = await fixture.CreateStudentAsync(
            "draft-detail-leader@example.com", "Dana", "Draft", "S9");
        var team = await fixture.CreateTeamAsync("Draft Detail Team", "DDR0001", leader);
        var draft = await fixture.CreateProposalAsync(
            team, "Draft Detail", ProjectStatuses.Draft, null);

        var draftResult = await fixture.Service.GetProposalAsync(draft.Id);
        var missingResult = await fixture.Service.GetProposalAsync(999999);

        Assert.Equal(AdminProposalError.NotFound, draftResult.Error);
        Assert.Equal("proposal_not_found", draftResult.ErrorCode);
        Assert.Equal("proposal_not_found", missingResult.ErrorCode);
    }

    [Fact]
    public async Task ReviewProposal_ApproveSetsDecisionReviewerAndClearsRejectionReason()
    {
        await using var fixture = await Fixture.CreateAsync();
        var admin = await fixture.CreateAdminAsync();
        var leader = await fixture.CreateStudentAsync(
            "approve-leader@example.com", "Alia", "Approve", "S10");
        var team = await fixture.CreateTeamAsync("Approve Team", "APR0001", leader);
        var project = await fixture.CreateProposalAsync(
            team, "Approve Me", ProjectStatuses.PendingReview, DateTime.UtcNow.AddDays(-1));
        project.RejectionReason = "stale rejection";
        await fixture.Db.SaveChangesAsync();

        var result = await fixture.Service.ReviewProposalAsync(
            admin.Id,
            project.Id,
            new ReviewProjectProposalDto
            {
                Decision = ProjectStatuses.Approved,
                Reason = "  Looks good  "
            },
            "127.0.0.1");

        Assert.True(result.Succeeded);
        var stored = await fixture.Db.Projects.AsNoTracking().SingleAsync(item => item.Id == project.Id);
        Assert.Equal(ProjectStatuses.Approved, stored.Status);
        Assert.Equal("Looks good", stored.ReviewReason);
        Assert.Null(stored.RejectionReason);
        Assert.Equal(admin.Id, stored.ReviewedByUserId);
        Assert.NotNull(stored.ReviewedAt);
        Assert.Equal(ProjectStatuses.Approved, result.Value!.Review!.Decision);
        Assert.Equal("Looks good", result.Value.Review.Reason);
        Assert.Equal("Ada Admin", result.Value.Review.ReviewedByName);
    }

    [Fact]
    public async Task ReviewProposal_ApproveWithoutReasonSucceedsAndStoresNullReason()
    {
        await using var fixture = await Fixture.CreateAsync();
        var admin = await fixture.CreateAdminAsync();
        var leader = await fixture.CreateStudentAsync(
            "approve-no-reason-leader@example.com", "Nora", "NoReason", "S16");
        var team = await fixture.CreateTeamAsync("No Reason Team", "NOR0001", leader);
        var project = await fixture.CreateProposalAsync(
            team, "Approve Without Reason", ProjectStatuses.PendingReview, DateTime.UtcNow.AddDays(-1));

        var result = await fixture.Service.ReviewProposalAsync(
            admin.Id,
            project.Id,
            new ReviewProjectProposalDto
            {
                Decision = ProjectStatuses.Approved,
                Reason = "   "
            },
            null);

        Assert.True(result.Succeeded);
        var stored = await fixture.Db.Projects.AsNoTracking().SingleAsync(item => item.Id == project.Id);
        Assert.Equal(ProjectStatuses.Approved, stored.Status);
        Assert.Null(stored.ReviewReason);
        Assert.Null(stored.RejectionReason);
        Assert.Null(result.Value!.Review!.Reason);
    }

    [Fact]
    public async Task ReviewProposal_RejectRequiresReasonAndSynchronizesRejectionReason()
    {
        await using var fixture = await Fixture.CreateAsync();
        var admin = await fixture.CreateAdminAsync();
        var leader = await fixture.CreateStudentAsync(
            "reject-leader@example.com", "Rami", "Reject", "S11");
        var team = await fixture.CreateTeamAsync("Reject Team", "REJ0001", leader);
        var project = await fixture.CreateProposalAsync(
            team, "Reject Me", ProjectStatuses.PendingReview, DateTime.UtcNow.AddDays(-1));

        var missingReason = await fixture.Service.ReviewProposalAsync(
            admin.Id,
            project.Id,
            new ReviewProjectProposalDto { Decision = ProjectStatuses.Rejected, Reason = "   " },
            null);
        var rejected = await fixture.Service.ReviewProposalAsync(
            admin.Id,
            project.Id,
            new ReviewProjectProposalDto
            {
                Decision = ProjectStatuses.Rejected,
                Reason = "  Missing requirements  "
            },
            null);

        Assert.Equal(AdminProposalError.InvalidRequest, missingReason.Error);
        Assert.Equal("rejection_reason_required", missingReason.ErrorCode);
        Assert.True(rejected.Succeeded);
        var stored = await fixture.Db.Projects.AsNoTracking().SingleAsync(item => item.Id == project.Id);
        Assert.Equal(ProjectStatuses.Rejected, stored.Status);
        Assert.Equal("Missing requirements", stored.ReviewReason);
        Assert.Equal("Missing requirements", stored.RejectionReason);
    }

    [Fact]
    public async Task ReviewProposal_NonPendingProposalReturnsConflictWithoutChanges()
    {
        await using var fixture = await Fixture.CreateAsync();
        var admin = await fixture.CreateAdminAsync();
        var leader = await fixture.CreateStudentAsync(
            "conflict-leader@example.com", "Cora", "Conflict", "S12");
        var team = await fixture.CreateTeamAsync("Conflict Team", "CNF0001", leader);
        var project = await fixture.CreateProposalAsync(
            team,
            "Already Decided",
            ProjectStatuses.Approved,
            DateTime.UtcNow.AddDays(-2),
            DateTime.UtcNow.AddDays(-1));

        var result = await fixture.Service.ReviewProposalAsync(
            admin.Id,
            project.Id,
            new ReviewProjectProposalDto { Decision = ProjectStatuses.Rejected, Reason = "Too late" },
            null);

        Assert.Equal(AdminProposalError.Conflict, result.Error);
        Assert.Equal("proposal_not_pending_review", result.ErrorCode);
        Assert.Empty(await fixture.Db.AuditLogs.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task ReviewProposal_InvalidDecisionReturnsInvalidDecision()
    {
        await using var fixture = await Fixture.CreateAsync();
        var admin = await fixture.CreateAdminAsync();
        var leader = await fixture.CreateStudentAsync(
            "decision-leader@example.com", "Dina", "Decision", "S13");
        var team = await fixture.CreateTeamAsync("Decision Team", "DEC0001", leader);
        var project = await fixture.CreateProposalAsync(
            team, "Decision Pending", ProjectStatuses.PendingReview, DateTime.UtcNow.AddDays(-1));

        var result = await fixture.Service.ReviewProposalAsync(
            admin.Id,
            project.Id,
            new ReviewProjectProposalDto { Decision = "Maybe" },
            null);

        Assert.Equal(AdminProposalError.InvalidRequest, result.Error);
        Assert.Equal("invalid_decision", result.ErrorCode);
    }

    [Fact]
    public async Task ReviewProposal_WritesAuditRowWithActionAndWithoutSensitiveText()
    {
        await using var fixture = await Fixture.CreateAsync();
        var admin = await fixture.CreateAdminAsync();
        var leader = await fixture.CreateStudentAsync(
            "audit-leader@example.com", "Aida", "Audit", "S14");
        var team = await fixture.CreateTeamAsync("Audit Team", "AUD0001", leader);
        var project = await fixture.CreateProposalAsync(
            team, "Audit Pending", ProjectStatuses.PendingReview, DateTime.UtcNow.AddDays(-1));

        await fixture.Service.ReviewProposalAsync(
            admin.Id,
            project.Id,
            new ReviewProjectProposalDto
            {
                Decision = ProjectStatuses.Rejected,
                Reason = "sensitive rejection reason"
            },
            "10.0.0.5");

        var log = Assert.Single(await fixture.Db.AuditLogs.AsNoTracking().ToListAsync());
        Assert.Equal("ProposalRejected", log.Action);
        Assert.Equal("Project", log.EntityName);
        Assert.Equal(project.Id.ToString(), log.EntityId);
        Assert.Equal(admin.Id, log.UserId);
        Assert.Equal("10.0.0.5", log.IpAddress);
        Assert.DoesNotContain("sensitive rejection reason", log.Description);
    }

    [Fact]
    public async Task ReviewProposal_UsesReviewerArgumentNotRequestContent()
    {
        await using var fixture = await Fixture.CreateAsync();
        var actingAdmin = await fixture.CreateAdminAsync("acting-admin@example.com");
        var otherAdmin = await fixture.CreateAdminAsync("other-admin@example.com");
        var leader = await fixture.CreateStudentAsync(
            "reviewer-leader@example.com", "Rana", "Reviewer", "S15");
        var team = await fixture.CreateTeamAsync("Reviewer Team", "RVW0001", leader);
        var project = await fixture.CreateProposalAsync(
            team, "Reviewer Pending", ProjectStatuses.PendingReview, DateTime.UtcNow.AddDays(-1));

        var result = await fixture.Service.ReviewProposalAsync(
            actingAdmin.Id,
            project.Id,
            new ReviewProjectProposalDto { Decision = ProjectStatuses.Approved },
            null);

        Assert.True(result.Succeeded);
        Assert.Equal(actingAdmin.Id, result.Value!.Review!.ReviewedByUserId);
        Assert.NotEqual(otherAdmin.Id, result.Value.Review.ReviewedByUserId);
        var log = Assert.Single(await fixture.Db.AuditLogs.AsNoTracking().ToListAsync());
        Assert.Equal(actingAdmin.Id, log.UserId);
    }

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ServiceProvider _provider;

        public ApplicationDbContext Db { get; }
        public AdminProposalService Service { get; }

        private Fixture(SqliteConnection connection, ServiceProvider provider, ApplicationDbContext db)
        {
            _connection = connection;
            _provider = provider;
            Db = db;
            Service = new AdminProposalService(
                new Repository<Project>(db),
                new Repository<Team>(db),
                new Repository<TeamMember>(db),
                new Repository<StudentProfile>(db),
                new Repository<ApplicationUser>(db),
                new Repository<AuditLog>(db));
        }

        public static async Task<Fixture> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection));
            var provider = services.BuildServiceProvider();
            var db = provider.GetRequiredService<ApplicationDbContext>();
            await db.Database.EnsureCreatedAsync();
            return new Fixture(connection, provider, db);
        }

        public async Task<ApplicationUser> CreateAdminAsync(string email = "admin@example.com")
        {
            var admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = "Ada",
                LastName = "Admin"
            };
            Db.Users.Add(admin);
            await Db.SaveChangesAsync();
            return admin;
        }

        public async Task<StudentProfile> CreateStudentAsync(
            string email,
            string firstName,
            string lastName,
            string studentNumber)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName
            };
            Db.Users.Add(user);
            await Db.SaveChangesAsync();

            var profile = new StudentProfile
            {
                UserId = user.Id,
                StudentNumber = studentNumber,
                Department = "Computer Science",
                AcademicYear = "Year 3"
            };
            Db.StudentProfiles.Add(profile);
            await Db.SaveChangesAsync();
            return profile;
        }

        public async Task<Team> CreateTeamAsync(
            string name,
            string teamCode,
            params StudentProfile[] students)
        {
            var team = new Team
            {
                Name = name,
                TeamCode = teamCode,
                MaxMembers = 5,
                CreatedByStudentId = students.FirstOrDefault()?.Id
            };
            Db.Teams.Add(team);
            await Db.SaveChangesAsync();

            for (var index = 0; index < students.Length; index++)
            {
                Db.TeamMembers.Add(new TeamMember
                {
                    TeamId = team.Id,
                    StudentId = students[index].Id,
                    IsLeader = index == 0,
                    JoinedAt = DateTime.UtcNow.AddMinutes(index)
                });
            }
            await Db.SaveChangesAsync();
            return team;
        }

        public async Task<Project> CreateProposalAsync(
            Team team,
            string title,
            string status,
            DateTime? submittedAt,
            DateTime? reviewedAt = null)
        {
            var project = new Project
            {
                TeamId = team.Id,
                Title = title,
                Description = $"{title} description",
                Status = status,
                SubmittedAt = submittedAt,
                ReviewedAt = reviewedAt
            };
            Db.Projects.Add(project);
            await Db.SaveChangesAsync();
            return project;
        }

        public async ValueTask DisposeAsync()
        {
            await Db.DisposeAsync();
            await _provider.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
