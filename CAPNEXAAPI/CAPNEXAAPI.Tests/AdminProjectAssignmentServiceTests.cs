using CAPNEXAAPI.DTOs.Request;
using CAPNEXAAPI.Models;
using CAPNEXAAPI.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CAPNEXAAPI.Tests;

public sealed class AdminProjectAssignmentServiceTests
{
    [Fact]
    public async Task GetUnassignedProjects_EmptyTable_ReturnsEmptyPage()
    {
        await using var fixture = await AdminSupervisorAssignmentFixture.CreateAsync();

        var result = await fixture.AssignmentService.GetUnassignedProjectsAsync(
            new AdminUnassignedProjectsQueryDto());

        Assert.True(result.Succeeded);
        Assert.Empty(result.Value!.Items);
        Assert.Equal(1, result.Value.Page);
        Assert.Equal(20, result.Value.PageSize);
        Assert.Equal(0, result.Value.TotalCount);
        Assert.Equal(0, result.Value.TotalPages);
    }

    [Fact]
    public async Task GetUnassignedProjects_ReturnsOnlyApprovedWithoutSupervisor()
    {
        await using var fixture = await AdminSupervisorAssignmentFixture.CreateAsync();
        var supervisor = await fixture.CreateSupervisorAsync(
            "unassigned-supervisor@example.com", "Uma", "Supervisor", 3);
        var leader = await fixture.CreateStudentAsync(
            "unassigned-leader@example.com", "Lina", "Leader", "S30");
        var member = await fixture.CreateStudentAsync(
            "unassigned-member@example.com", "Mona", "Member", "S31");

        var approvedTeam = await fixture.CreateTeamAsync("Approved Team", "UNA0001", leader, member);
        var assignedTeam = await fixture.CreateTeamAsync("Assigned Team", "UNA0002");
        var draftTeam = await fixture.CreateTeamAsync("Draft Team", "UNA0003");
        var pendingTeam = await fixture.CreateTeamAsync("Pending Team", "UNA0004");
        var rejectedTeam = await fixture.CreateTeamAsync("Rejected Team", "UNA0005");

        var approved = await fixture.CreateProjectAsync(
            approvedTeam, "Waiting Approval", ProjectStatuses.Approved,
            DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1));
        await fixture.CreateProjectAsync(
            assignedTeam, "Already Assigned", ProjectStatuses.Approved,
            DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1), supervisor.Id);
        await fixture.CreateProjectAsync(
            draftTeam, "Draft Project", ProjectStatuses.Draft);
        await fixture.CreateProjectAsync(
            pendingTeam, "Pending Project", ProjectStatuses.PendingReview,
            DateTime.UtcNow.AddDays(-1));
        await fixture.CreateProjectAsync(
            rejectedTeam, "Rejected Project", ProjectStatuses.Rejected,
            DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1));

        var result = await fixture.AssignmentService.GetUnassignedProjectsAsync(
            new AdminUnassignedProjectsQueryDto());

        Assert.True(result.Succeeded);
        var item = Assert.Single(result.Value!.Items);
        Assert.Equal(approved.Id, item.ProjectId);
        Assert.Equal("Waiting Approval", item.Title);
        Assert.NotNull(item.ReviewedAt);
        Assert.Equal("Approved Team", item.Team.Name);
        Assert.Equal("UNA0001", item.Team.TeamCode);
        Assert.Equal(2, item.Team.MemberCount);
        Assert.Equal("Lina Leader", item.LeaderName);
        Assert.Equal(1, result.Value.TotalCount);
    }

    [Fact]
    public async Task GetUnassignedProjects_OrdersByReviewedAtThenId()
    {
        await using var fixture = await AdminSupervisorAssignmentFixture.CreateAsync();
        var newerTeam = await fixture.CreateTeamAsync("Newer Team", "ORD0001");
        var olderFirstTeam = await fixture.CreateTeamAsync("Older One", "ORD0002");
        var olderSecondTeam = await fixture.CreateTeamAsync("Older Two", "ORD0003");

        var newer = await fixture.CreateProjectAsync(
            newerTeam, "Newer Waiting", ProjectStatuses.Approved,
            DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1));
        var olderFirst = await fixture.CreateProjectAsync(
            olderFirstTeam, "Older Waiting One", ProjectStatuses.Approved,
            DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(-4));
        var olderSecond = await fixture.CreateProjectAsync(
            olderSecondTeam, "Older Waiting Two", ProjectStatuses.Approved,
            DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(-4));

        var result = await fixture.AssignmentService.GetUnassignedProjectsAsync(
            new AdminUnassignedProjectsQueryDto());

        Assert.Equal(
            [olderFirst.Id, olderSecond.Id, newer.Id],
            result.Value!.Items.Select(item => item.ProjectId).ToArray());
    }

    [Fact]
    public async Task GetUnassignedProjects_SearchesTitleTeamNameAndTeamCode()
    {
        await using var fixture = await AdminSupervisorAssignmentFixture.CreateAsync();
        var navigatorTeam = await fixture.CreateTeamAsync("Rocket Team", "RCK900");
        var trackerTeam = await fixture.CreateTeamAsync("Ocean Team", "OCN901");

        await fixture.CreateProjectAsync(
            navigatorTeam, "Campus Navigator", ProjectStatuses.Approved,
            DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1));
        await fixture.CreateProjectAsync(
            trackerTeam, "Ocean Tracker", ProjectStatuses.Approved,
            DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1));

        var byTitle = await fixture.AssignmentService.GetUnassignedProjectsAsync(
            new AdminUnassignedProjectsQueryDto { Search = "navigator" });
        var byTeamName = await fixture.AssignmentService.GetUnassignedProjectsAsync(
            new AdminUnassignedProjectsQueryDto { Search = "ocean" });
        var byTeamCode = await fixture.AssignmentService.GetUnassignedProjectsAsync(
            new AdminUnassignedProjectsQueryDto { Search = "rck9" });

        Assert.Equal("Campus Navigator", Assert.Single(byTitle.Value!.Items).Title);
        Assert.Equal("Ocean Tracker", Assert.Single(byTeamName.Value!.Items).Title);
        Assert.Equal("Campus Navigator", Assert.Single(byTeamCode.Value!.Items).Title);
    }

    [Fact]
    public async Task GetUnassignedProjects_InvalidPagination_ReturnsBusinessError()
    {
        await using var fixture = await AdminSupervisorAssignmentFixture.CreateAsync();

        var badPage = await fixture.AssignmentService.GetUnassignedProjectsAsync(
            new AdminUnassignedProjectsQueryDto { Page = 0 });
        var badPageSize = await fixture.AssignmentService.GetUnassignedProjectsAsync(
            new AdminUnassignedProjectsQueryDto { PageSize = 0 });
        var tooLarge = await fixture.AssignmentService.GetUnassignedProjectsAsync(
            new AdminUnassignedProjectsQueryDto
            {
                PageSize = AdminProjectAssignmentService.MaximumPageSize + 1
            });

        Assert.Equal(AdminProjectAssignmentError.InvalidRequest, badPage.Error);
        Assert.Equal("invalid_pagination", badPage.ErrorCode);
        Assert.Equal("invalid_pagination", badPageSize.ErrorCode);
        Assert.Equal("invalid_pagination", tooLarge.ErrorCode);
    }

    [Fact]
    public async Task AssignSupervisor_AssignsProjectWritesAuditAndComputesCounts()
    {
        await using var fixture = await AdminSupervisorAssignmentFixture.CreateAsync();
        var admin = await fixture.CreateAdminAsync();
        var supervisor = await fixture.CreateSupervisorAsync(
            "assign-supervisor@example.com", "Sara", "Assign", 2);
        var leader = await fixture.CreateStudentAsync(
            "assign-leader@example.com", "Lina", "Leader", "S34");
        var member = await fixture.CreateStudentAsync(
            "assign-member@example.com", "Mona", "Member", "S35");
        var team = await fixture.CreateTeamAsync("Assign Team", "ASN0001", leader, member);
        var project = await fixture.CreateProjectAsync(
            team, "Assign Me", ProjectStatuses.Approved,
            DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1));

        var result = await fixture.AssignmentService.AssignSupervisorAsync(
            admin.Id,
            project.Id,
            new AssignSupervisorDto { SupervisorId = supervisor.Id },
            "10.4.5.6");

        Assert.True(result.Succeeded);
        var value = result.Value!;
        Assert.Equal(project.Id, value.ProjectId);
        Assert.Equal("Assign Me", value.Title);
        Assert.Equal(ProjectStatuses.Approved, value.Status);
        Assert.Equal(team.Id, value.Team.TeamId);
        Assert.Equal("Assign Team", value.Team.Name);
        Assert.Equal("ASN0001", value.Team.TeamCode);
        Assert.Equal(2, value.Team.MemberCount);
        Assert.Equal(supervisor.Id, value.Supervisor.SupervisorId);
        Assert.Equal(supervisor.UserId, value.Supervisor.UserId);
        Assert.Equal("Sara", value.Supervisor.FirstName);
        Assert.Equal("Assign", value.Supervisor.LastName);
        Assert.Equal("assign-supervisor@example.com", value.Supervisor.Email);
        Assert.Equal(2, value.Supervisor.MaxProjects);
        Assert.Equal(1, value.Supervisor.AssignedProjectsCount);
        Assert.Equal(1, value.Supervisor.AvailableCapacity);

        var stored = await fixture.Db.Projects.AsNoTracking()
            .SingleAsync(item => item.Id == project.Id);
        Assert.Equal(supervisor.Id, stored.SupervisorId);
        Assert.Equal("Assign Me", stored.Title);
        Assert.Equal(ProjectStatuses.Approved, stored.Status);
        Assert.Equal(project.ReviewedAt, stored.ReviewedAt);

        var log = Assert.Single(await fixture.Db.AuditLogs.AsNoTracking().ToListAsync());
        Assert.Equal("SupervisorAssigned", log.Action);
        Assert.Equal("Project", log.EntityName);
        Assert.Equal(project.Id.ToString(), log.EntityId);
        Assert.Equal(admin.Id, log.UserId);
        Assert.Equal("10.4.5.6", log.IpAddress);
        Assert.False(string.IsNullOrWhiteSpace(log.Description));
    }

    [Fact]
    public async Task AssignSupervisor_SameSupervisor_IsNoOpWithoutAudit()
    {
        await using var fixture = await AdminSupervisorAssignmentFixture.CreateAsync();
        var admin = await fixture.CreateAdminAsync();
        var supervisor = await fixture.CreateSupervisorAsync(
            "same-supervisor@example.com", "Sami", "Same", 1);
        var leader = await fixture.CreateStudentAsync(
            "same-leader@example.com", "Lina", "Leader", "S36");
        var team = await fixture.CreateTeamAsync("Same Team", "SAM0001", leader);
        var project = await fixture.CreateProjectAsync(
            team, "Same Project", ProjectStatuses.Approved,
            DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1), supervisor.Id);

        var result = await fixture.AssignmentService.AssignSupervisorAsync(
            admin.Id,
            project.Id,
            new AssignSupervisorDto { SupervisorId = supervisor.Id },
            "10.7.7.7");

        Assert.True(result.Succeeded);
        Assert.Equal(1, result.Value!.Supervisor.AssignedProjectsCount);
        Assert.Equal(0, result.Value.Supervisor.AvailableCapacity);
        Assert.Empty(await fixture.Db.AuditLogs.AsNoTracking().ToListAsync());
        var stored = await fixture.Db.Projects.AsNoTracking()
            .SingleAsync(item => item.Id == project.Id);
        Assert.Equal(supervisor.Id, stored.SupervisorId);
    }

    [Fact]
    public async Task AssignSupervisor_ReassignmentMovesProjectAndUpdatesBothCounts()
    {
        await using var fixture = await AdminSupervisorAssignmentFixture.CreateAsync();
        var admin = await fixture.CreateAdminAsync();
        var previous = await fixture.CreateSupervisorAsync(
            "previous-supervisor@example.com", "Paula", "Previous", 1);
        var next = await fixture.CreateSupervisorAsync(
            "next-supervisor@example.com", "Nate", "Next", 2);
        var leader = await fixture.CreateStudentAsync(
            "reassign-leader@example.com", "Lina", "Leader", "S37");
        var team = await fixture.CreateTeamAsync("Reassign Team", "REA0001", leader);
        var project = await fixture.CreateProjectAsync(
            team, "Reassign Me", ProjectStatuses.Approved,
            DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1), previous.Id);

        var result = await fixture.AssignmentService.AssignSupervisorAsync(
            admin.Id,
            project.Id,
            new AssignSupervisorDto { SupervisorId = next.Id },
            "10.8.8.8");

        Assert.True(result.Succeeded);
        Assert.Equal(next.Id, result.Value!.Supervisor.SupervisorId);
        Assert.Equal(1, result.Value.Supervisor.AssignedProjectsCount);
        Assert.Equal(1, result.Value.Supervisor.AvailableCapacity);

        var stored = await fixture.Db.Projects.AsNoTracking()
            .SingleAsync(item => item.Id == project.Id);
        Assert.Equal(next.Id, stored.SupervisorId);

        var previousDetail = await fixture.SupervisorService.GetSupervisorAsync(previous.Id);
        Assert.Equal(0, previousDetail.Value!.AssignedProjectsCount);

        var nextDetail = await fixture.SupervisorService.GetSupervisorAsync(next.Id);
        Assert.Equal(1, nextDetail.Value!.AssignedProjectsCount);

        var log = Assert.Single(await fixture.Db.AuditLogs.AsNoTracking().ToListAsync());
        Assert.Equal("SupervisorReassigned", log.Action);
        Assert.Equal("Project", log.EntityName);
        Assert.Equal(project.Id.ToString(), log.EntityId);
        Assert.Equal(admin.Id, log.UserId);
        Assert.Equal("10.8.8.8", log.IpAddress);
    }

    [Fact]
    public async Task AssignSupervisor_MissingProject_ReturnsNotFound()
    {
        await using var fixture = await AdminSupervisorAssignmentFixture.CreateAsync();
        var admin = await fixture.CreateAdminAsync();
        var supervisor = await fixture.CreateSupervisorAsync(
            "missing-project-supervisor@example.com", "Mia", "Missing", 2);

        var result = await fixture.AssignmentService.AssignSupervisorAsync(
            admin.Id,
            999,
            new AssignSupervisorDto { SupervisorId = supervisor.Id },
            null);

        Assert.Equal(AdminProjectAssignmentError.NotFound, result.Error);
        Assert.Equal("project_not_found", result.ErrorCode);
    }

    [Fact]
    public async Task AssignSupervisor_NonApprovedProject_ReturnsConflict()
    {
        await using var fixture = await AdminSupervisorAssignmentFixture.CreateAsync();
        var admin = await fixture.CreateAdminAsync();
        var supervisor = await fixture.CreateSupervisorAsync(
            "non-approved-supervisor@example.com", "Nina", "Approve", 2);
        var draftTeam = await fixture.CreateTeamAsync("Draft Assign", "NON0001");
        var pendingTeam = await fixture.CreateTeamAsync("Pending Assign", "NON0002");
        var rejectedTeam = await fixture.CreateTeamAsync("Rejected Assign", "NON0003");
        var draft = await fixture.CreateProjectAsync(
            draftTeam, "Draft Assign", ProjectStatuses.Draft);
        var pending = await fixture.CreateProjectAsync(
            pendingTeam, "Pending Assign", ProjectStatuses.PendingReview,
            DateTime.UtcNow.AddDays(-1));
        var rejected = await fixture.CreateProjectAsync(
            rejectedTeam, "Rejected Assign", ProjectStatuses.Rejected,
            DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1));

        foreach (var project in new[] { draft, pending, rejected })
        {
            var result = await fixture.AssignmentService.AssignSupervisorAsync(
                admin.Id,
                project.Id,
                new AssignSupervisorDto { SupervisorId = supervisor.Id },
                null);

            Assert.Equal(AdminProjectAssignmentError.Conflict, result.Error);
            Assert.Equal("project_not_approved", result.ErrorCode);
        }

        Assert.All(
            await fixture.Db.Projects.AsNoTracking().ToListAsync(),
            project => Assert.Null(project.SupervisorId));
        Assert.Empty(await fixture.Db.AuditLogs.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task AssignSupervisor_MissingSupervisor_ReturnsNotFound()
    {
        await using var fixture = await AdminSupervisorAssignmentFixture.CreateAsync();
        var admin = await fixture.CreateAdminAsync();
        var leader = await fixture.CreateStudentAsync(
            "no-supervisor-leader@example.com", "Lina", "Leader", "S39");
        var team = await fixture.CreateTeamAsync("No Supervisor Team", "NOS0001", leader);
        var project = await fixture.CreateProjectAsync(
            team, "No Supervisor Project", ProjectStatuses.Approved,
            DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1));

        var result = await fixture.AssignmentService.AssignSupervisorAsync(
            admin.Id,
            project.Id,
            new AssignSupervisorDto { SupervisorId = 999 },
            null);

        Assert.Equal(AdminProjectAssignmentError.NotFound, result.Error);
        Assert.Equal("supervisor_not_found", result.ErrorCode);
    }

    [Fact]
    public async Task AssignSupervisor_InactiveSupervisor_ReturnsConflict()
    {
        await using var fixture = await AdminSupervisorAssignmentFixture.CreateAsync();
        var admin = await fixture.CreateAdminAsync();
        var supervisor = await fixture.CreateSupervisorAsync(
            "inactive-supervisor@example.com", "Ivan", "Inactive", 2,
            isActive: false);
        var leader = await fixture.CreateStudentAsync(
            "inactive-leader@example.com", "Lina", "Leader", "S40");
        var team = await fixture.CreateTeamAsync("Inactive Team", "INA0001", leader);
        var project = await fixture.CreateProjectAsync(
            team, "Inactive Project", ProjectStatuses.Approved,
            DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1));

        var result = await fixture.AssignmentService.AssignSupervisorAsync(
            admin.Id,
            project.Id,
            new AssignSupervisorDto { SupervisorId = supervisor.Id },
            null);

        Assert.Equal(AdminProjectAssignmentError.Conflict, result.Error);
        Assert.Equal("supervisor_inactive", result.ErrorCode);
        Assert.Empty(await fixture.Db.AuditLogs.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task AssignSupervisor_SupervisorWithZeroCapacity_ReturnsCapacityReached()
    {
        await using var fixture = await AdminSupervisorAssignmentFixture.CreateAsync();
        var admin = await fixture.CreateAdminAsync();
        var supervisor = await fixture.CreateSupervisorAsync(
            "zero-capacity@example.com", "Zoe", "Zero", 0);
        var leader = await fixture.CreateStudentAsync(
            "zero-capacity-leader@example.com", "Lina", "Leader", "S41");
        var team = await fixture.CreateTeamAsync("Zero Capacity Team", "ZER0001", leader);
        var project = await fixture.CreateProjectAsync(
            team, "Zero Capacity Project", ProjectStatuses.Approved,
            DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1));

        var result = await fixture.AssignmentService.AssignSupervisorAsync(
            admin.Id,
            project.Id,
            new AssignSupervisorDto { SupervisorId = supervisor.Id },
            null);

        Assert.Equal(AdminProjectAssignmentError.Conflict, result.Error);
        Assert.Equal("supervisor_capacity_reached", result.ErrorCode);
        var stored = await fixture.Db.Projects.AsNoTracking()
            .SingleAsync(item => item.Id == project.Id);
        Assert.Null(stored.SupervisorId);
        Assert.Empty(await fixture.Db.AuditLogs.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task AssignSupervisor_CapacityReached_ReturnsConflictAndLeavesProjectUnassigned()
    {
        await using var fixture = await AdminSupervisorAssignmentFixture.CreateAsync();
        var admin = await fixture.CreateAdminAsync();
        var supervisor = await fixture.CreateSupervisorAsync(
            "full-capacity@example.com", "Faye", "Full", 1);
        var existingTeam = await fixture.CreateTeamAsync("Existing Team", "FUL0001");
        var targetTeam = await fixture.CreateTeamAsync("Target Team", "FUL0002");
        await fixture.CreateProjectAsync(
            existingTeam, "Existing Project", ProjectStatuses.Approved,
            DateTime.UtcNow.AddDays(-3), DateTime.UtcNow.AddDays(-2), supervisor.Id);
        var target = await fixture.CreateProjectAsync(
            targetTeam, "Target Project", ProjectStatuses.Approved,
            DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1));

        var result = await fixture.AssignmentService.AssignSupervisorAsync(
            admin.Id,
            target.Id,
            new AssignSupervisorDto { SupervisorId = supervisor.Id },
            null);

        Assert.Equal(AdminProjectAssignmentError.Conflict, result.Error);
        Assert.Equal("supervisor_capacity_reached", result.ErrorCode);
        var stored = await fixture.Db.Projects.AsNoTracking()
            .SingleAsync(item => item.Id == target.Id);
        Assert.Null(stored.SupervisorId);
        Assert.Empty(await fixture.Db.AuditLogs.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task AssignSupervisor_InvalidPayload_ReturnsBadRequestAndChangesNothing()
    {
        await using var fixture = await AdminSupervisorAssignmentFixture.CreateAsync();
        var admin = await fixture.CreateAdminAsync();
        var leader = await fixture.CreateStudentAsync(
            "invalid-assign-leader@example.com", "Lina", "Leader", "S43");
        var team = await fixture.CreateTeamAsync("Invalid Assign Team", "INV0001", leader);
        var project = await fixture.CreateProjectAsync(
            team, "Invalid Assign Project", ProjectStatuses.Approved,
            DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1));

        var nullBody = await fixture.AssignmentService.AssignSupervisorAsync(
            admin.Id, project.Id, null, null);
        var missing = await fixture.AssignmentService.AssignSupervisorAsync(
            admin.Id, project.Id, new AssignSupervisorDto(), null);
        var zero = await fixture.AssignmentService.AssignSupervisorAsync(
            admin.Id, project.Id, new AssignSupervisorDto { SupervisorId = 0 }, null);

        Assert.Equal(AdminProjectAssignmentError.InvalidRequest, nullBody.Error);
        Assert.Equal("invalid_supervisor_payload", nullBody.ErrorCode);
        Assert.Equal("invalid_supervisor_payload", missing.ErrorCode);
        Assert.Equal("invalid_supervisor_payload", zero.ErrorCode);

        var stored = await fixture.Db.Projects.AsNoTracking()
            .SingleAsync(item => item.Id == project.Id);
        Assert.Null(stored.SupervisorId);
        Assert.Empty(await fixture.Db.AuditLogs.AsNoTracking().ToListAsync());
    }
}
