using CAPNEXAAPI.DTOs.Response;
using CAPNEXAAPI.Models;
using CAPNEXAAPI.Repositories;

namespace CAPNEXAAPI.Services;

public sealed class StudentDashboardService : IStudentDashboardService
{
    private readonly IRepository<StudentProfile> _students;
    private readonly IRepository<Team> _teams;
    private readonly IRepository<TeamMember> _teamMembers;
    private readonly IRepository<Project> _projects;
    private readonly IRepository<Milestone> _milestones;
    private readonly IRepository<SupervisorProfile> _supervisors;

    public StudentDashboardService(
        IRepository<StudentProfile> students,
        IRepository<Team> teams,
        IRepository<TeamMember> teamMembers,
        IRepository<Project> projects,
        IRepository<Milestone> milestones,
        IRepository<SupervisorProfile> supervisors)
    {
        _students = students;
        _teams = teams;
        _teamMembers = teamMembers;
        _projects = projects;
        _milestones = milestones;
        _supervisors = supervisors;
    }

    public async Task<StudentDashboardResult<StudentDashboardDto>> GetDashboardAsync(string userId)
    {
        var access = await GetAccessAsync(userId);
        if (access.Error != StudentDashboardError.None)
            return new(null, access.Error);

        var student = access.Student!;

        // Get team info if student has one
        TeamDashboardDto? teamDto = null;
        var membership = await _teamMembers.GetOneAsync(m => m.StudentId == student.Id, IsTracking: false);

        if (membership is not null)
        {
            teamDto = await BuildTeamDtoAsync(membership.TeamId, student.Id);
        }

        // Get proposal info if team exists and has a proposal
        ProposalDashboardDto? proposalDto = null;
        ProjectDashboardDto? projectDto = null;
        IReadOnlyList<MilestoneDashboardDto> milestones = [];
        int progressPercentage = 0;
        IReadOnlyList<UpcomingDeadlineDto> upcomingDeadlines = [];

        if (teamDto is not null)
        {
            // Get the latest proposal for this team
            var proposal = await _projects.GetOneAsync(
                p => p.TeamId == teamDto.TeamId,
                [p => p.Milestones],
                IsTracking: false);

            if (proposal is not null)
            {
                proposalDto = new ProposalDashboardDto
                {
                    ProjectId = proposal.Id,
                    Title = proposal.Title,
                    Status = proposal.Status,
                    RejectionReason = proposal.RejectionReason,
                    CreatedAt = proposal.CreatedAt,
                    SubmittedAt = proposal.SubmittedAt
                };

                // If proposal is approved, it becomes a project
                if (proposal.Status == ProjectStatuses.Approved)
                {
                    var supervisor = proposal.SupervisorId.HasValue
                        ? await _supervisors.GetOneAsync(s => s.Id == proposal.SupervisorId.Value, IsTracking: false)
                        : null;

                    projectDto = new ProjectDashboardDto
                    {
                        ProjectId = proposal.Id,
                        Title = proposal.Title,
                        Status = proposal.Status,
                        CreatedAt = proposal.CreatedAt,
                        SupervisorId = proposal.SupervisorId,
                        SupervisorName = supervisor != null ? $"{supervisor.User.FirstName} {supervisor.User.LastName}" : null
                    };
                }

                // Get milestones
                var milestoneList = proposal.Milestones
                    .OrderBy(m => m.Order)
                    .Select(m => new MilestoneDashboardDto
                    {
                        MilestoneId = m.Id,
                        Title = m.Title,
                        Description = m.Description,
                        DueDate = m.DueDate,
                        Order = m.Order,
                        Status = m.Status,
                        CreatedAt = m.CreatedAt
                    })
                    .ToArray();

                milestones = milestoneList;

                // Calculate progress percentage: completed milestones / total milestones
                var totalMilestones = milestoneList.Length;
                if (totalMilestones > 0)
                {
                    var completedMilestones = milestoneList.Count(m => m.Status == "Completed");
                    progressPercentage = (completedMilestones * 100) / totalMilestones;
                }

                // Upcoming deadlines: milestones with due dates that are not completed, ordered by due date
                upcomingDeadlines = milestoneList
                    .Where(m => m.DueDate.HasValue && m.Status != "Completed")
                    .OrderBy(m => m.DueDate)
                    .Select(m => new UpcomingDeadlineDto
                    {
                        MilestoneId = m.MilestoneId,
                        Title = m.Title,
                        DueDate = m.DueDate!.Value,
                        Order = m.Order
                    })
                    .ToArray();
            }
        }

        var dashboard = new StudentDashboardDto
        {
            Team = teamDto,
            Proposal = proposalDto,
            Project = projectDto,
            Milestones = milestones,
            ProgressPercentage = progressPercentage,
            UpcomingDeadlines = upcomingDeadlines
        };

        return new(dashboard);
    }

    private async Task<(StudentProfile? Student, StudentDashboardError Error)> GetAccessAsync(string userId)
    {
        var student = await _students.GetOneAsync(
            profile => profile.UserId == userId,
            [profile => profile.User],
            IsTracking: false);

        if (student is null ||
            string.IsNullOrWhiteSpace(student.StudentNumber) ||
            string.IsNullOrWhiteSpace(student.Department) ||
            string.IsNullOrWhiteSpace(student.AcademicYear))
            return (null, StudentDashboardError.ProfileRequired);

        if (!student.User.IsActive)
            return (null, StudentDashboardError.AccountDisabled);

        return (student, StudentDashboardError.None);
    }

    private async Task<TeamDashboardDto> BuildTeamDtoAsync(int teamId, int currentStudentId)
    {
        var team = await _teams.GetOneAsync(
            t => t.Id == teamId,
            [t => t.Members],
            IsTracking: false);

        var memberIds = team!.Members.Select(m => m.StudentId).ToArray();
        var students = await _students.GetAllAsync(
            s => memberIds.Contains(s.Id),
            [s => s.User],
            IsTracking: false);
        var studentsById = students.ToDictionary(s => s.Id);

        var leader = team.Members.FirstOrDefault(m => m.IsLeader);
        var leaderStudent = leader != null && studentsById.TryGetValue(leader.StudentId, out var ls) ? ls : null;

        return new TeamDashboardDto
        {
            TeamId = team.Id,
            Name = team.Name,
            TeamCode = team.TeamCode,
            MaxMembers = team.MaxMembers,
            MemberCount = team.Members.Count,
            Leader = leaderStudent != null ? new TeamLeaderDto
            {
                UserId = leaderStudent.UserId,
                FirstName = leaderStudent.User.FirstName,
                LastName = leaderStudent.User.LastName,
                StudentNumber = leaderStudent.StudentNumber
            } : new TeamLeaderDto(),
            Members = team.Members.Select(m => studentsById.TryGetValue(m.StudentId, out var s) ? new TeamMemberDashboardDto
            {
                UserId = s.UserId,
                FirstName = s.User.FirstName,
                LastName = s.User.LastName,
                StudentNumber = s.StudentNumber,
                IsLeader = m.IsLeader,
                JoinedAt = m.JoinedAt
            } : new TeamMemberDashboardDto()).ToArray()
        };
    }
}