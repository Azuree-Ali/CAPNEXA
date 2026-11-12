using CAPNEXAAPI.DTOs.Response;
using CAPNEXAAPI.Models;
using CAPNEXAAPI.Repositories;

namespace CAPNEXAAPI.Services;

public enum TeamError
{
    None,
    ProfileRequired,
    AccountDisabled,
    StudentAlreadyInTeam,
    TeamNotFound,
    TeamFull,
    NotTeamLeader
}

public sealed record TeamResult<T>(T? Value, TeamError Error = TeamError.None)
{
    public bool Succeeded => Error == TeamError.None;
}

public sealed class TeamService : ITeamService
{
    public const int MaximumMembers = 5;

    private readonly IRepository<Team> _teams;
    private readonly IRepository<TeamMember> _members;
    private readonly IRepository<StudentProfile> _students;

    public TeamService(
        IRepository<Team> teams,
        IRepository<TeamMember> members,
        IRepository<StudentProfile> students)
    {
        _teams = teams;
        _members = members;
        _students = students;
    }

    public async Task<TeamResult<TeamDto>> CreateAsync(string userId, string name)
    {
        var student = await GetEligibleStudentAsync(userId);
        if (student is null)
            return new(null, TeamError.ProfileRequired);
        if (!student.User.IsActive)
            return new(null, TeamError.AccountDisabled);

        if (await HasTeamAsync(student.Id))
            return new(null, TeamError.StudentAlreadyInTeam);

        var team = new Team
        {
            Name = name.Trim(),
            TeamCode = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpperInvariant(),
            CreatedByStudentId = student.Id,
            AcademicYear = student.AcademicYear,
            MaxMembers = MaximumMembers
        };

        await _teams.CreateAsync(team);
        await _teams.CommitAsync();
        await _members.CreateAsync(new TeamMember
        {
            TeamId = team.Id,
            StudentId = student.Id,
            IsLeader = true
        });
        await _members.CommitAsync();

        return new(await GetTeamDtoAsync(team.Id));
    }

    public async Task<TeamResult<TeamDto>> GetMineAsync(string userId)
    {
        var student = await GetEligibleStudentAsync(userId);
        if (student is null)
            return new(null, TeamError.ProfileRequired);
        if (!student.User.IsActive)
            return new(null, TeamError.AccountDisabled);

        var membership = await _members.GetOneAsync(member => member.StudentId == student.Id);

        return membership is null
            ? new(null, TeamError.TeamNotFound)
            : await GetTeamResultAsync(membership.TeamId);
    }

    public async Task<TeamResult<TeamDto>> JoinAsync(string userId, string teamCode)
    {
        var student = await GetEligibleStudentAsync(userId);
        if (student is null)
            return new(null, TeamError.ProfileRequired);
        if (!student.User.IsActive)
            return new(null, TeamError.AccountDisabled);

        if (await HasTeamAsync(student.Id))
            return new(null, TeamError.StudentAlreadyInTeam);

        var team = await _teams.GetOneAsync(item => item.TeamCode == teamCode.Trim());
        if (team is null)
            return new(null, TeamError.TeamNotFound);

        var members = await _members.GetAllAsync(member => member.TeamId == team.Id);
        if (members.Count() >= team.MaxMembers)
            return new(null, TeamError.TeamFull);

        await _members.CreateAsync(new TeamMember { TeamId = team.Id, StudentId = student.Id });
        await _members.CommitAsync();

        return await GetTeamResultAsync(team.Id);
    }

    public async Task<TeamResult<bool>> LeaveAsync(string userId)
    {
        var student = await GetEligibleStudentAsync(userId);
        if (student is null)
            return new(false, TeamError.ProfileRequired);
        if (!student.User.IsActive)
            return new(false, TeamError.AccountDisabled);

        var membership = await _members.GetOneAsync(member => member.StudentId == student.Id);

        if (membership is null)
            return new(false, TeamError.TeamNotFound);
        if (membership.IsLeader)
            return new(false, TeamError.NotTeamLeader);

        _members.Delete(membership);
        await _members.CommitAsync();
        return new(true);
    }

    private async Task<StudentProfile?> GetEligibleStudentAsync(string userId)
    {
        var student = await _students.GetOneAsync(profile => profile.UserId == userId, [profile => profile.User]);
        if (student is null)
            return null;

        if (string.IsNullOrWhiteSpace(student.StudentNumber) ||
            string.IsNullOrWhiteSpace(student.Department) ||
            string.IsNullOrWhiteSpace(student.AcademicYear))
            return null;

        return student;
    }

    private async Task<bool> HasTeamAsync(int studentId)
        => await _members.GetOneAsync(member => member.StudentId == studentId) is not null;

    private async Task<TeamResult<TeamDto>> GetTeamResultAsync(int teamId)
        => new(await GetTeamDtoAsync(teamId));

    private async Task<TeamDto> GetTeamDtoAsync(int teamId)
    {
        var team = await _teams.GetOneAsync(item => item.Id == teamId, [item => item.Members], IsTracking: false);
        var studentIds = team!.Members.Select(member => member.StudentId).ToArray();
        var students = await _students.GetAllAsync(student => studentIds.Contains(student.Id), [student => student.User], IsTracking: false);
        var studentsById = students.ToDictionary(student => student.Id);

        return new TeamDto
        {
            TeamId = team.Id,
            Name = team.Name,
            TeamCode = team.TeamCode,
            MaxMembers = team.MaxMembers,
            MemberCount = team.Members.Count,
            Members = team.Members.Select(member => new TeamMemberDto
            {
                UserId = studentsById[member.StudentId].UserId,
                FirstName = studentsById[member.StudentId].User.FirstName,
                LastName = studentsById[member.StudentId].User.LastName,
                StudentNumber = studentsById[member.StudentId].StudentNumber,
                IsLeader = member.IsLeader,
                JoinedAt = member.JoinedAt
            }).OrderByDescending(member => member.IsLeader).ThenBy(member => member.JoinedAt).ToArray()
        };
    }
}
