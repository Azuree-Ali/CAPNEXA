using CAPNEXAAPI.DTOs.Request;
using CAPNEXAAPI.Models;
using CAPNEXAAPI.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CAPNEXAAPI.Tests;

public sealed class AdminSupervisorServiceTests
{
    [Fact]
    public async Task GetSupervisors_EmptyTable_ReturnsEmptyPage()
    {
        await using var fixture = await AdminSupervisorAssignmentFixture.CreateAsync();

        var result = await fixture.SupervisorService.GetSupervisorsAsync(
            new AdminSupervisorQueryDto());

        Assert.True(result.Succeeded);
        Assert.Empty(result.Value!.Items);
        Assert.Equal(1, result.Value.Page);
        Assert.Equal(20, result.Value.PageSize);
        Assert.Equal(0, result.Value.TotalCount);
        Assert.Equal(0, result.Value.TotalPages);
    }

    [Fact]
    public async Task GetSupervisors_ReturnsProjectCountsCapacityAndNullProfileText()
    {
        await using var fixture = await AdminSupervisorAssignmentFixture.CreateAsync();
        var full = await fixture.CreateSupervisorAsync(
            "full@example.com",
            "Fiona",
            "Full",
            2,
            department: "Computer Science",
            specialization: "Artificial Intelligence",
            researchInterests: "Robotics");
        var empty = await fixture.CreateSupervisorAsync(
            "empty@example.com",
            "Evan",
            "Empty",
            3);

        var firstTeam = await fixture.CreateTeamAsync("Counts One", "CNT0001");
        var secondTeam = await fixture.CreateTeamAsync("Counts Two", "CNT0002");
        await fixture.CreateProjectAsync(
            firstTeam, "First Assigned", ProjectStatuses.Approved,
            DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1), full.Id);
        await fixture.CreateProjectAsync(
            secondTeam, "Second Assigned", ProjectStatuses.Approved,
            DateTime.UtcNow.AddDays(-1), DateTime.UtcNow, full.Id);

        var result = await fixture.SupervisorService.GetSupervisorsAsync(
            new AdminSupervisorQueryDto());

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Value!.TotalCount);

        var fullItem = result.Value.Items.Single(item => item.Email == "full@example.com");
        Assert.Equal(full.Id, fullItem.SupervisorId);
        Assert.Equal(full.UserId, fullItem.UserId);
        Assert.True(fullItem.IsActive);
        Assert.Equal("Fiona", fullItem.FirstName);
        Assert.Equal("Full", fullItem.LastName);
        Assert.Equal("Computer Science", fullItem.Department);
        Assert.Equal("Artificial Intelligence", fullItem.Specialization);
        Assert.Equal("Robotics", fullItem.ResearchInterests);
        Assert.Equal(2, fullItem.MaxProjects);
        Assert.Equal(2, fullItem.AssignedProjectsCount);
        Assert.Equal(0, fullItem.AvailableCapacity);

        var emptyItem = result.Value.Items.Single(item => item.Email == "empty@example.com");
        Assert.Equal(empty.Id, emptyItem.SupervisorId);
        Assert.Null(emptyItem.Department);
        Assert.Null(emptyItem.Specialization);
        Assert.Null(emptyItem.ResearchInterests);
        Assert.Equal(3, emptyItem.MaxProjects);
        Assert.Equal(0, emptyItem.AssignedProjectsCount);
        Assert.Equal(3, emptyItem.AvailableCapacity);
    }

    [Fact]
    public async Task GetSupervisors_FiltersBySearchDepartmentIsActiveAndCapacity()
    {
        await using var fixture = await AdminSupervisorAssignmentFixture.CreateAsync();
        await fixture.CreateSupervisorAsync(
            "alice.smith@example.com", "Alice", "Smith", 2,
            department: "Computer Science",
            specialization: "Artificial Intelligence");
        await fixture.CreateSupervisorAsync(
            "bob.jones@example.com", "Bob", "Jones", 0,
            isActive: false,
            department: "Mathematics");
        await fixture.CreateSupervisorAsync(
            "carol.white@example.com", "Carol", "White", 1,
            specialization: "Data Science");

        var service = fixture.SupervisorService;

        var byFirstName = await service.GetSupervisorsAsync(
            new AdminSupervisorQueryDto { Search = "alice" });
        var byLastName = await service.GetSupervisorsAsync(
            new AdminSupervisorQueryDto { Search = "jones" });
        var byEmail = await service.GetSupervisorsAsync(
            new AdminSupervisorQueryDto { Search = "carol.white@" });
        var byDepartmentSearch = await service.GetSupervisorsAsync(
            new AdminSupervisorQueryDto { Search = "computer" });
        var bySpecialization = await service.GetSupervisorsAsync(
            new AdminSupervisorQueryDto { Search = "data science" });
        var byDepartment = await service.GetSupervisorsAsync(
            new AdminSupervisorQueryDto { Department = "Mathematics" });
        var inactive = await service.GetSupervisorsAsync(
            new AdminSupervisorQueryDto { IsActive = false });
        var withCapacity = await service.GetSupervisorsAsync(
            new AdminSupervisorQueryDto { HasCapacity = true });
        var withoutCapacity = await service.GetSupervisorsAsync(
            new AdminSupervisorQueryDto { HasCapacity = false });

        Assert.Equal("alice.smith@example.com", Assert.Single(byFirstName.Value!.Items).Email);
        Assert.Equal("bob.jones@example.com", Assert.Single(byLastName.Value!.Items).Email);
        Assert.Equal("carol.white@example.com", Assert.Single(byEmail.Value!.Items).Email);
        Assert.Equal("alice.smith@example.com", Assert.Single(byDepartmentSearch.Value!.Items).Email);
        Assert.Equal("carol.white@example.com", Assert.Single(bySpecialization.Value!.Items).Email);
        Assert.Equal("bob.jones@example.com", Assert.Single(byDepartment.Value!.Items).Email);
        Assert.False(Assert.Single(inactive.Value!.Items).IsActive);
        Assert.Equal(
            new[] { "alice.smith@example.com", "carol.white@example.com" },
            withCapacity.Value!.Items
                .Select(item => item.Email ?? string.Empty)
                .OrderBy(email => email)
                .ToArray());
        Assert.Equal(
            "bob.jones@example.com",
            Assert.Single(withoutCapacity.Value!.Items).Email);
    }

    [Fact]
    public async Task GetSupervisors_OrdersByLastNameFirstNameThenId()
    {
        await using var fixture = await AdminSupervisorAssignmentFixture.CreateAsync();
        await fixture.CreateSupervisorAsync("zeta@example.com", "Zeta", "Zenith", 1);
        await fixture.CreateSupervisorAsync("zoe@example.com", "Zoe", "Adams", 1);
        await fixture.CreateSupervisorAsync("amy@example.com", "Amy", "Adams", 1);

        var result = await fixture.SupervisorService.GetSupervisorsAsync(
            new AdminSupervisorQueryDto());

        Assert.Equal(
            ["Amy Adams", "Zoe Adams", "Zeta Zenith"],
            result.Value!.Items
                .Select(item => $"{item.FirstName} {item.LastName}")
                .ToArray());
    }

    [Fact]
    public async Task GetSupervisors_PaginatesResults()
    {
        await using var fixture = await AdminSupervisorAssignmentFixture.CreateAsync();
        await fixture.CreateSupervisorAsync("one@example.com", "Amy", "Alpha", 1);
        await fixture.CreateSupervisorAsync("two@example.com", "Amy", "Bravo", 1);
        await fixture.CreateSupervisorAsync("three@example.com", "Amy", "Charlie", 1);

        var result = await fixture.SupervisorService.GetSupervisorsAsync(
            new AdminSupervisorQueryDto { Page = 2, PageSize = 2 });

        Assert.Equal("Charlie", Assert.Single(result.Value!.Items).LastName);
        Assert.Equal(3, result.Value.TotalCount);
        Assert.Equal(2, result.Value.TotalPages);
        Assert.Equal(2, result.Value.Page);
        Assert.Equal(2, result.Value.PageSize);
    }

    [Fact]
    public async Task GetSupervisors_InvalidPagination_ReturnsBusinessError()
    {
        await using var fixture = await AdminSupervisorAssignmentFixture.CreateAsync();

        var badPage = await fixture.SupervisorService.GetSupervisorsAsync(
            new AdminSupervisorQueryDto { Page = 0 });
        var badPageSize = await fixture.SupervisorService.GetSupervisorsAsync(
            new AdminSupervisorQueryDto { PageSize = 0 });
        var tooLarge = await fixture.SupervisorService.GetSupervisorsAsync(
            new AdminSupervisorQueryDto { PageSize = AdminSupervisorService.MaximumPageSize + 1 });

        Assert.Equal(AdminSupervisorError.InvalidRequest, badPage.Error);
        Assert.Equal("invalid_pagination", badPage.ErrorCode);
        Assert.Equal("invalid_pagination", badPageSize.ErrorCode);
        Assert.Equal("invalid_pagination", tooLarge.ErrorCode);
    }

    [Fact]
    public async Task GetSupervisor_ReturnsAssignedProjectsOrderedById()
    {
        await using var fixture = await AdminSupervisorAssignmentFixture.CreateAsync();
        var supervisor = await fixture.CreateSupervisorAsync(
            "detail-supervisor@example.com", "Dina", "Detail", 3);
        var other = await fixture.CreateSupervisorAsync(
            "other-supervisor@example.com", "Omar", "Other", 3);

        var firstTeam = await fixture.CreateTeamAsync("Alpha Team", "ALP0001");
        var secondTeam = await fixture.CreateTeamAsync("Beta Team", "BET0002");
        var otherTeam = await fixture.CreateTeamAsync("Other Team", "OTH0003");

        var first = await fixture.CreateProjectAsync(
            firstTeam, "Alpha Project", ProjectStatuses.Approved,
            DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1), supervisor.Id);
        var second = await fixture.CreateProjectAsync(
            secondTeam, "Beta Project", ProjectStatuses.Approved,
            DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1), supervisor.Id);
        await fixture.CreateProjectAsync(
            otherTeam, "Other Project", ProjectStatuses.Approved,
            DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1), other.Id);

        var result = await fixture.SupervisorService.GetSupervisorAsync(supervisor.Id);

        Assert.True(result.Succeeded);
        var value = result.Value!;
        Assert.Equal(supervisor.Id, value.SupervisorId);
        Assert.Equal(2, value.AssignedProjectsCount);
        Assert.Equal(1, value.AvailableCapacity);
        Assert.Equal([first.Id, second.Id], value.AssignedProjects.Select(item => item.ProjectId).ToArray());
        Assert.Equal("Alpha Team", value.AssignedProjects[0].TeamName);
        Assert.Equal("ALP0001", value.AssignedProjects[0].TeamCode);
        Assert.Equal(firstTeam.Id, value.AssignedProjects[0].TeamId);
        Assert.Equal(ProjectStatuses.Approved, value.AssignedProjects[0].Status);
    }

    [Fact]
    public async Task GetSupervisor_Missing_ReturnsNotFound()
    {
        await using var fixture = await AdminSupervisorAssignmentFixture.CreateAsync();

        var result = await fixture.SupervisorService.GetSupervisorAsync(999);

        Assert.Equal(AdminSupervisorError.NotFound, result.Error);
        Assert.Equal("supervisor_not_found", result.ErrorCode);
    }

    [Fact]
    public async Task UpdateMaxProjects_ChangesOnlyCapacityAndWritesAudit()
    {
        await using var fixture = await AdminSupervisorAssignmentFixture.CreateAsync();
        var admin = await fixture.CreateAdminAsync();
        var supervisor = await fixture.CreateSupervisorAsync(
            "capacity@example.com", "Cora", "Capacity", 5,
            department: "Computer Science",
            specialization: "Security",
            researchInterests: "Cryptography");
        var firstTeam = await fixture.CreateTeamAsync("Capacity One", "CAP0001");
        var secondTeam = await fixture.CreateTeamAsync("Capacity Two", "CAP0002");
        await fixture.CreateProjectAsync(
            firstTeam, "Capacity Project One", ProjectStatuses.Approved,
            DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1), supervisor.Id);
        await fixture.CreateProjectAsync(
            secondTeam, "Capacity Project Two", ProjectStatuses.Approved,
            DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1), supervisor.Id);

        var beforeUser = await fixture.Db.Users.AsNoTracking()
            .SingleAsync(user => user.Id == supervisor.UserId);
        var beforeProjects = await fixture.Db.Projects.AsNoTracking()
            .OrderBy(project => project.Id)
            .Select(project => new { project.Id, project.SupervisorId, project.Status, project.Title })
            .ToListAsync();

        var result = await fixture.SupervisorService.UpdateMaxProjectsAsync(
            admin.Id,
            supervisor.Id,
            new UpdateSupervisorCapacityDto { MaxProjects = 3 },
            "10.1.2.3");

        Assert.True(result.Succeeded);
        Assert.Equal(3, result.Value!.MaxProjects);
        Assert.Equal(2, result.Value.AssignedProjectsCount);
        Assert.Equal(1, result.Value.AvailableCapacity);

        var stored = await fixture.Db.SupervisorProfiles.AsNoTracking()
            .SingleAsync(item => item.Id == supervisor.Id);
        Assert.Equal(3, stored.MaxProjects);
        Assert.Equal(supervisor.UserId, stored.UserId);
        Assert.Equal("Computer Science", stored.Department);
        Assert.Equal("Security", stored.Specialization);
        Assert.Equal("Cryptography", stored.ResearchInterests);

        var afterUser = await fixture.Db.Users.AsNoTracking()
            .SingleAsync(user => user.Id == supervisor.UserId);
        Assert.Equal(beforeUser.FirstName, afterUser.FirstName);
        Assert.Equal(beforeUser.LastName, afterUser.LastName);
        Assert.Equal(beforeUser.Email, afterUser.Email);
        Assert.Equal(beforeUser.IsActive, afterUser.IsActive);

        var afterProjects = await fixture.Db.Projects.AsNoTracking()
            .OrderBy(project => project.Id)
            .Select(project => new { project.Id, project.SupervisorId, project.Status, project.Title })
            .ToListAsync();
        Assert.Equal(beforeProjects, afterProjects);

        var log = Assert.Single(await fixture.Db.AuditLogs.AsNoTracking().ToListAsync());
        Assert.Equal("SupervisorCapacityUpdated", log.Action);
        Assert.Equal("SupervisorProfile", log.EntityName);
        Assert.Equal(supervisor.Id.ToString(), log.EntityId);
        Assert.Equal(admin.Id, log.UserId);
        Assert.Equal("10.1.2.3", log.IpAddress);
        Assert.False(string.IsNullOrWhiteSpace(log.Description));
    }

    [Fact]
    public async Task UpdateMaxProjects_ValueEqualToAssignedCountIsAllowed()
    {
        await using var fixture = await AdminSupervisorAssignmentFixture.CreateAsync();
        var admin = await fixture.CreateAdminAsync();
        var supervisor = await fixture.CreateSupervisorAsync(
            "equal@example.com", "Eve", "Equal", 5);
        var firstTeam = await fixture.CreateTeamAsync("Equal One", "EQL0001");
        var secondTeam = await fixture.CreateTeamAsync("Equal Two", "EQL0002");
        await fixture.CreateProjectAsync(
            firstTeam, "Equal One", ProjectStatuses.Approved,
            DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1), supervisor.Id);
        await fixture.CreateProjectAsync(
            secondTeam, "Equal Two", ProjectStatuses.Approved,
            DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1), supervisor.Id);

        var result = await fixture.SupervisorService.UpdateMaxProjectsAsync(
            admin.Id,
            supervisor.Id,
            new UpdateSupervisorCapacityDto { MaxProjects = 2 },
            null);

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Value!.MaxProjects);
        Assert.Equal(0, result.Value.AvailableCapacity);
        Assert.Single(await fixture.Db.AuditLogs.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task UpdateMaxProjects_BelowAssignedCount_ReturnsConflictAndChangesNothing()
    {
        await using var fixture = await AdminSupervisorAssignmentFixture.CreateAsync();
        var admin = await fixture.CreateAdminAsync();
        var supervisor = await fixture.CreateSupervisorAsync(
            "below@example.com", "Bea", "Below", 5);
        var firstTeam = await fixture.CreateTeamAsync("Below One", "BLW0001");
        var secondTeam = await fixture.CreateTeamAsync("Below Two", "BLW0002");
        await fixture.CreateProjectAsync(
            firstTeam, "Below One", ProjectStatuses.Approved,
            DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1), supervisor.Id);
        await fixture.CreateProjectAsync(
            secondTeam, "Below Two", ProjectStatuses.Approved,
            DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1), supervisor.Id);

        var result = await fixture.SupervisorService.UpdateMaxProjectsAsync(
            admin.Id,
            supervisor.Id,
            new UpdateSupervisorCapacityDto { MaxProjects = 1 },
            null);

        Assert.Equal(AdminSupervisorError.Conflict, result.Error);
        Assert.Equal("capacity_below_assigned_projects", result.ErrorCode);
        var stored = await fixture.Db.SupervisorProfiles.AsNoTracking()
            .SingleAsync(item => item.Id == supervisor.Id);
        Assert.Equal(5, stored.MaxProjects);
        Assert.Empty(await fixture.Db.AuditLogs.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task UpdateMaxProjects_SameValue_IsNoOpWithoutAudit()
    {
        await using var fixture = await AdminSupervisorAssignmentFixture.CreateAsync();
        var admin = await fixture.CreateAdminAsync();
        var supervisor = await fixture.CreateSupervisorAsync(
            "same@example.com", "Sam", "Same", 3);

        var result = await fixture.SupervisorService.UpdateMaxProjectsAsync(
            admin.Id,
            supervisor.Id,
            new UpdateSupervisorCapacityDto { MaxProjects = 3 },
            "10.9.9.9");

        Assert.True(result.Succeeded);
        Assert.Equal(3, result.Value!.MaxProjects);
        var stored = await fixture.Db.SupervisorProfiles.AsNoTracking()
            .SingleAsync(item => item.Id == supervisor.Id);
        Assert.Equal(3, stored.MaxProjects);
        Assert.Empty(await fixture.Db.AuditLogs.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task UpdateMaxProjects_MissingOrOutOfRangeValue_ReturnsBadRequestAndChangesNothing()
    {
        await using var fixture = await AdminSupervisorAssignmentFixture.CreateAsync();
        var admin = await fixture.CreateAdminAsync();
        var supervisor = await fixture.CreateSupervisorAsync(
            "invalid@example.com", "Iris", "Invalid", 4);

        var missing = await fixture.SupervisorService.UpdateMaxProjectsAsync(
            admin.Id, supervisor.Id, new UpdateSupervisorCapacityDto(), null);
        var nullBody = await fixture.SupervisorService.UpdateMaxProjectsAsync(
            admin.Id, supervisor.Id, null, null);
        var negative = await fixture.SupervisorService.UpdateMaxProjectsAsync(
            admin.Id,
            supervisor.Id,
            new UpdateSupervisorCapacityDto { MaxProjects = -1 },
            null);
        var tooLarge = await fixture.SupervisorService.UpdateMaxProjectsAsync(
            admin.Id,
            supervisor.Id,
            new UpdateSupervisorCapacityDto { MaxProjects = AdminSupervisorService.MaximumProjects + 1 },
            null);

        Assert.Equal(AdminSupervisorError.InvalidRequest, missing.Error);
        Assert.Equal("invalid_capacity_payload", missing.ErrorCode);
        Assert.Equal("invalid_capacity_payload", nullBody.ErrorCode);
        Assert.Equal("invalid_capacity_payload", negative.ErrorCode);
        Assert.Equal("invalid_capacity_payload", tooLarge.ErrorCode);

        var stored = await fixture.Db.SupervisorProfiles.AsNoTracking()
            .SingleAsync(item => item.Id == supervisor.Id);
        Assert.Equal(4, stored.MaxProjects);
        Assert.Empty(await fixture.Db.AuditLogs.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task UpdateMaxProjects_MissingSupervisor_ReturnsNotFound()
    {
        await using var fixture = await AdminSupervisorAssignmentFixture.CreateAsync();
        var admin = await fixture.CreateAdminAsync();

        var result = await fixture.SupervisorService.UpdateMaxProjectsAsync(
            admin.Id,
            999,
            new UpdateSupervisorCapacityDto { MaxProjects = 2 },
            null);

        Assert.Equal(AdminSupervisorError.NotFound, result.Error);
        Assert.Equal("supervisor_not_found", result.ErrorCode);
    }

    [Fact]
    public async Task UpdateMaxProjects_AllowsZeroWhenNoProjectsAssigned()
    {
        await using var fixture = await AdminSupervisorAssignmentFixture.CreateAsync();
        var admin = await fixture.CreateAdminAsync();
        var supervisor = await fixture.CreateSupervisorAsync(
            "zero@example.com", "Zara", "Zero", 5);

        var result = await fixture.SupervisorService.UpdateMaxProjectsAsync(
            admin.Id,
            supervisor.Id,
            new UpdateSupervisorCapacityDto { MaxProjects = 0 },
            null);

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.Value!.MaxProjects);
        Assert.Equal(0, result.Value.AvailableCapacity);
        var stored = await fixture.Db.SupervisorProfiles.AsNoTracking()
            .SingleAsync(item => item.Id == supervisor.Id);
        Assert.Equal(0, stored.MaxProjects);
    }
}
