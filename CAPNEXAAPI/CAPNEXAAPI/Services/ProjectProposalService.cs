using CAPNEXAAPI.DTOs.Request;
using CAPNEXAAPI.DTOs.Response;
using CAPNEXAAPI.Models;
using CAPNEXAAPI.Repositories;

namespace CAPNEXAAPI.Services;

public sealed class ProjectProposalService : IProjectProposalService
{
    private readonly IRepository<Project> _projects;
    private readonly IRepository<StudentProfile> _students;
    private readonly IRepository<TeamMember> _members;

    public ProjectProposalService(
        IRepository<Project> projects,
        IRepository<StudentProfile> students,
        IRepository<TeamMember> members)
    {
        _projects = projects;
        _students = students;
        _members = members;
    }

    public async Task<ProjectProposalResult<IReadOnlyList<ProjectProposalDto>>> GetMineAsync(string userId)
    {
        var access = await GetTeamAccessAsync(userId);
        if (access.Error != ProjectProposalError.None)
            return new(null, access.Error);

        var projects = await _projects.GetAllAsync(project => project.TeamId == access.TeamId, IsTracking: false);
        var result = projects.OrderByDescending(project => project.CreatedAt)
            .Select(ToDto)
            .ToArray();

        return new(result);
    }

    public async Task<ProjectProposalResult<ProjectProposalDto>> CreateAsync(string userId, CreateProjectProposalDto request)
    {
        var access = await GetTeamAccessAsync(userId);
        if (access.Error != ProjectProposalError.None)
            return new(null, access.Error);

        var activeProposal = await _projects.GetOneAsync(
            project => project.TeamId == access.TeamId && project.Status != ProjectStatuses.Rejected,
            IsTracking: false);
        if (activeProposal is not null)
            return new(null, ProjectProposalError.ActiveProposalExists);

        var project = new Project
        {
            TeamId = access.TeamId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Status = ProjectStatuses.Draft
        };

        await _projects.CreateAsync(project);
        await _projects.CommitAsync();

        return new(ToDto(project));
    }

    public async Task<ProjectProposalResult<ProjectProposalDto>> UpdateAsync(string userId, int projectId, CreateProjectProposalDto request)
    {
        var access = await GetTeamAccessAsync(userId);
        if (access.Error != ProjectProposalError.None)
            return new(null, access.Error);

        var project = await _projects.GetOneAsync(item => item.Id == projectId && item.TeamId == access.TeamId);
        if (project is null)
            return new(null, ProjectProposalError.ProposalNotFound);
        if (project.Status != ProjectStatuses.Draft)
            return new(null, ProjectProposalError.ProposalNotEditable);

        project.Title = request.Title.Trim();
        project.Description = request.Description.Trim();
        _projects.Update(project);
        await _projects.CommitAsync();

        return new(ToDto(project));
    }

    public async Task<ProjectProposalResult<ProjectProposalDto>> SubmitAsync(string userId, int projectId)
    {
        var access = await GetTeamAccessAsync(userId, requireLeader: true);
        if (access.Error != ProjectProposalError.None)
            return new(null, access.Error);

        var project = await _projects.GetOneAsync(item => item.Id == projectId && item.TeamId == access.TeamId);
        if (project is null)
            return new(null, ProjectProposalError.ProposalNotFound);
        if (project.Status != ProjectStatuses.Draft)
            return new(null, ProjectProposalError.ProposalNotEditable);
        if (string.IsNullOrWhiteSpace(project.Title) || string.IsNullOrWhiteSpace(project.Description))
            return new(null, ProjectProposalError.ProposalNotReady);

        project.Status = ProjectStatuses.PendingReview;
        project.SubmittedAt = DateTime.UtcNow;
        _projects.Update(project);
        await _projects.CommitAsync();

        return new(ToDto(project));
    }

    private async Task<(int TeamId, ProjectProposalError Error)> GetTeamAccessAsync(string userId, bool requireLeader = false)
    {
        var student = await _students.GetOneAsync(profile => profile.UserId == userId, [profile => profile.User]);
        if (student is null ||
            string.IsNullOrWhiteSpace(student.StudentNumber) ||
            string.IsNullOrWhiteSpace(student.Department) ||
            string.IsNullOrWhiteSpace(student.AcademicYear))
            return (0, ProjectProposalError.ProfileRequired);

        if (!student.User.IsActive)
            return (0, ProjectProposalError.AccountDisabled);

        var membership = await _members.GetOneAsync(member => member.StudentId == student.Id);
        if (membership is null)
            return (0, ProjectProposalError.TeamRequired);
        if (requireLeader && !membership.IsLeader)
            return (0, ProjectProposalError.NotTeamLeader);

        return (membership.TeamId, ProjectProposalError.None);
    }

    private static ProjectProposalDto ToDto(Project project)
        => new()
        {
            ProjectId = project.Id,
            TeamId = project.TeamId,
            Title = project.Title,
            Description = project.Description,
            Status = project.Status,
            RejectionReason = project.RejectionReason,
            CreatedAt = project.CreatedAt,
            SubmittedAt = project.SubmittedAt
        };
}
