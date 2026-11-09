using CAPNEXAAPI.DataAccess;
using CAPNEXAAPI.Models;
using CAPNEXAAPI.Repositories;
using CAPNEXAAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CAPNEXAAPI.Tests;

public sealed class TeamServiceTests
{
    [Fact]
    public async Task Create_AddsCreatorAsLeaderAndSetsFiveMemberLimit()
    {
        await using var fixture = await Fixture.CreateAsync();
        var userId = await fixture.CreateStudentAsync("1");

        var result = await fixture.Service.CreateAsync(userId, "Team One");

        Assert.True(result.Succeeded);
        Assert.Equal(5, result.Value!.MaxMembers);
        Assert.Equal(1, result.Value.MemberCount);
        Assert.True(result.Value.Members.Single().IsLeader);
    }

    [Fact]
    public async Task Join_RejectsSixthMemberAndSecondTeamMembership()
    {
        await using var fixture = await Fixture.CreateAsync();
        var owner = await fixture.CreateStudentAsync("1");
        var team = await fixture.Service.CreateAsync(owner, "Team One");

        for (var i = 2; i <= 5; i++)
        {
            var userId = await fixture.CreateStudentAsync(i.ToString());
            var joined = await fixture.Service.JoinAsync(userId, team.Value!.TeamCode);
            Assert.True(joined.Succeeded);
        }

        var sixth = await fixture.CreateStudentAsync("6");
        var fullResult = await fixture.Service.JoinAsync(sixth, team.Value!.TeamCode);
        var alreadyMemberResult = await fixture.Service.JoinAsync(owner, team.Value.TeamCode);

        Assert.Equal(TeamError.TeamFull, fullResult.Error);
        Assert.Equal(TeamError.StudentAlreadyInTeam, alreadyMemberResult.Error);
    }

    [Fact]
    public async Task Create_RequiresCompletedAcademicProfile()
    {
        await using var fixture = await Fixture.CreateAsync();
        var userId = await fixture.CreateStudentAsync("1", completeProfile: false);

        var result = await fixture.Service.CreateAsync(userId, "Team One");

        Assert.Equal(TeamError.ProfileRequired, result.Error);
    }

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ServiceProvider _provider;
        public ApplicationDbContext Db { get; }
        public TeamService Service { get; }

        private Fixture(SqliteConnection connection, ServiceProvider provider, ApplicationDbContext db)
        {
            _connection = connection;
            _provider = provider;
            Db = db;
            Service = new TeamService(
                new Repository<Team>(db),
                new Repository<TeamMember>(db),
                new Repository<StudentProfile>(db));
        }

        public static async Task<Fixture> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection));
            services.AddIdentityCore<ApplicationUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            var provider = services.BuildServiceProvider();
            var db = provider.GetRequiredService<ApplicationDbContext>();
            await db.Database.EnsureCreatedAsync();
            return new Fixture(connection, provider, db);
        }

        public async Task<string> CreateStudentAsync(string suffix, bool completeProfile = true)
        {
            var user = new ApplicationUser
            {
                UserName = $"student{suffix}@example.com",
                Email = $"student{suffix}@example.com",
                EmailConfirmed = true,
                FirstName = "Test",
                LastName = $"Student{suffix}"
            };
            var users = _provider.GetRequiredService<UserManager<ApplicationUser>>();
            Assert.True((await users.CreateAsync(user)).Succeeded);

            if (completeProfile)
            {
                Db.StudentProfiles.Add(new StudentProfile
                {
                    UserId = user.Id,
                    StudentNumber = $"S{suffix}",
                    Department = "Computer Science",
                    AcademicYear = "Year 3"
                });
                await Db.SaveChangesAsync();
            }

            return user.Id;
        }

        public async ValueTask DisposeAsync()
        {
            await Db.DisposeAsync();
            await _provider.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
