using CAPNEXAAPI.DataAccess;
using CAPNEXAAPI.Models;
using CAPNEXAAPI.Repositories;
using CAPNEXAAPI.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CAPNEXAAPI.Tests;

internal sealed class AdminSupervisorAssignmentFixture : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _provider;

    public ApplicationDbContext Db { get; }
    public AdminSupervisorService SupervisorService { get; }
    public AdminProjectAssignmentService AssignmentService { get; }

    private AdminSupervisorAssignmentFixture(
        SqliteConnection connection,
        ServiceProvider provider,
        ApplicationDbContext db)
    {
        _connection = connection;
        _provider = provider;
        Db = db;
        SupervisorService = new AdminSupervisorService(
            new Repository<SupervisorProfile>(db),
            new Repository<Project>(db),
            new Repository<AuditLog>(db));
        AssignmentService = new AdminProjectAssignmentService(
            new Repository<Project>(db),
            new Repository<SupervisorProfile>(db),
            new Repository<TeamMember>(db),
            new Repository<AuditLog>(db));
    }

    public static async Task<AdminSupervisorAssignmentFixture> CreateAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection));
        var provider = services.BuildServiceProvider();
        var db = provider.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync();
        return new AdminSupervisorAssignmentFixture(connection, provider, db);
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

    public async Task<SupervisorProfile> CreateSupervisorAsync(
        string email,
        string firstName,
        string lastName,
        int maxProjects,
        bool isActive = true,
        string? department = null,
        string? specialization = null,
        string? researchInterests = null)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName,
            IsActive = isActive
        };
        Db.Users.Add(user);
        await Db.SaveChangesAsync();

        var profile = new SupervisorProfile
        {
            UserId = user.Id,
            MaxProjects = maxProjects,
            Department = department,
            Specialization = specialization,
            ResearchInterests = researchInterests
        };
        Db.SupervisorProfiles.Add(profile);
        await Db.SaveChangesAsync();
        return profile;
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

    public async Task<Project> CreateProjectAsync(
        Team team,
        string title,
        string status,
        DateTime? submittedAt = null,
        DateTime? reviewedAt = null,
        int? supervisorId = null)
    {
        var project = new Project
        {
            TeamId = team.Id,
            SupervisorId = supervisorId,
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
