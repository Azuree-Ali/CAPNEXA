using CAPNEXAAPI.DTOs.Request;
using CAPNEXAAPI.DTOs.Response;
using CAPNEXAAPI.Models;
using CAPNEXAAPI.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CAPNEXAAPI.Services;

public sealed class AdminProjectAssignmentService : IAdminProjectAssignmentService
{
    public const int MaximumPageSize = 100;

    private const string LikeEscapeCharacter = "\\";

    private readonly IRepository<Project> _projects;
    private readonly IRepository<SupervisorProfile> _supervisors;
    private readonly IRepository<TeamMember> _members;
    private readonly IRepository<AuditLog> _auditLogs;

    public AdminProjectAssignmentService(
        IRepository<Project> projects,
        IRepository<SupervisorProfile> supervisors,
        IRepository<TeamMember> members,
        IRepository<AuditLog> auditLogs)
    {
        _projects = projects;
        _supervisors = supervisors;
        _members = members;
        _auditLogs = auditLogs;
    }

    public async Task<AdminProjectAssignmentResult<AdminUnassignedProjectListDto>>
        GetUnassignedProjectsAsync(AdminUnassignedProjectsQueryDto query)
    {
        if (query.Page < 1 || query.PageSize < 1 || query.PageSize > MaximumPageSize)
            return Failure<AdminUnassignedProjectListDto>(
                AdminProjectAssignmentError.InvalidRequest,
                "invalid_pagination");

        var projects = _projects.GetQueryable()
            .Where(project =>
                project.Status == ProjectStatuses.Approved &&
                project.SupervisorId == null);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = ContainsPattern(query.Search);
            projects = projects.Where(project =>
                EF.Functions.Like(project.Title, pattern, LikeEscapeCharacter) ||
                EF.Functions.Like(project.Team.Name, pattern, LikeEscapeCharacter) ||
                EF.Functions.Like(project.Team.TeamCode, pattern, LikeEscapeCharacter));
        }

        var totalCount = await projects.CountAsync();

        var rows = await projects
            .OrderBy(project => project.ReviewedAt)
            .ThenBy(project => project.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(project => new
            {
                ProjectId = project.Id,
                project.Title,
                project.ReviewedAt,
                TeamId = project.Team.Id,
                TeamName = project.Team.Name,
                TeamCode = project.Team.TeamCode,
                MemberCount = project.Team.Members.Count,
                LeaderName = project.Team.Members
                    .Where(member => member.IsLeader)
                    .Select(member =>
                        member.Student.User.FirstName + " " + member.Student.User.LastName)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Success(new AdminUnassignedProjectListDto
        {
            Items = rows.Select(row => new AdminUnassignedProjectListItemDto
            {
                ProjectId = row.ProjectId,
                Title = row.Title,
                ReviewedAt = row.ReviewedAt,
                LeaderName = row.LeaderName ?? string.Empty,
                Team = new AdminProposalTeamSummaryDto
                {
                    TeamId = row.TeamId,
                    Name = row.TeamName,
                    TeamCode = row.TeamCode,
                    MemberCount = row.MemberCount
                }
            }).ToArray(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        });
    }

    public async Task<AdminProjectAssignmentResult<AdminProjectAssignmentDto>>
        AssignSupervisorAsync(
            string adminUserId,
            int projectId,
            AssignSupervisorDto? request,
            string? ipAddress)
    {
        var project = await _projects.GetOneAsync(
            item => item.Id == projectId,
            [item => item.Team]);

        if (project is null)
            return Failure<AdminProjectAssignmentDto>(
                AdminProjectAssignmentError.NotFound,
                "project_not_found");

        if (project.Status != ProjectStatuses.Approved)
            return Failure<AdminProjectAssignmentDto>(
                AdminProjectAssignmentError.Conflict,
                "project_not_approved");

        if (request?.SupervisorId is null || request.SupervisorId < 1)
            return Failure<AdminProjectAssignmentDto>(
                AdminProjectAssignmentError.InvalidRequest,
                "invalid_supervisor_payload");

        var supervisorId = request.SupervisorId.Value;
        var supervisor = await _supervisors.GetOneAsync(
            item => item.Id == supervisorId,
            [item => item.User],
            IsTracking: false);

        if (supervisor is null)
            return Failure<AdminProjectAssignmentDto>(
                AdminProjectAssignmentError.NotFound,
                "supervisor_not_found");

        if (!supervisor.User.IsActive)
            return Failure<AdminProjectAssignmentDto>(
                AdminProjectAssignmentError.Conflict,
                "supervisor_inactive");

        if (project.SupervisorId == supervisor.Id)
            return Success(await BuildAssignmentAsync(project, supervisor));

        var assignedProjectsCount = await _projects.GetQueryable()
            .CountAsync(item => item.SupervisorId == supervisor.Id);

        if (assignedProjectsCount >= supervisor.MaxProjects)
            return Failure<AdminProjectAssignmentDto>(
                AdminProjectAssignmentError.Conflict,
                "supervisor_capacity_reached");

        var isReassignment = project.SupervisorId is not null;

        project.SupervisorId = supervisor.Id;
        _projects.Update(project);
        await _auditLogs.CreateAsync(new AuditLog
        {
            UserId = adminUserId,
            IpAddress = ipAddress,
            Action = isReassignment ? "SupervisorReassigned" : "SupervisorAssigned",
            EntityName = "Project",
            EntityId = projectId.ToString(),
            Description = isReassignment
                ? "Project assigned to a different supervisor."
                : "Supervisor assigned to project."
        });
        await _projects.CommitAsync();

        return Success(await BuildAssignmentAsync(project, supervisor));
    }

    private async Task<AdminProjectAssignmentDto> BuildAssignmentAsync(
        Project project,
        SupervisorProfile supervisor)
    {
        var assignedProjectsCount = await _projects.GetQueryable()
            .CountAsync(item => item.SupervisorId == supervisor.Id);

        var memberCount = await _members.GetQueryable()
            .CountAsync(member => member.TeamId == project.TeamId);

        return new AdminProjectAssignmentDto
        {
            ProjectId = project.Id,
            Title = project.Title,
            Status = project.Status,
            Team = new AdminProposalTeamSummaryDto
            {
                TeamId = project.TeamId,
                Name = project.Team?.Name ?? string.Empty,
                TeamCode = project.Team?.TeamCode ?? string.Empty,
                MemberCount = memberCount
            },
            Supervisor = new AdminProjectSupervisorDto
            {
                SupervisorId = supervisor.Id,
                UserId = supervisor.UserId,
                FirstName = supervisor.User.FirstName,
                LastName = supervisor.User.LastName,
                Email = supervisor.User.Email,
                MaxProjects = supervisor.MaxProjects,
                AssignedProjectsCount = assignedProjectsCount,
                AvailableCapacity = Math.Max(0, supervisor.MaxProjects - assignedProjectsCount)
            }
        };
    }

    private static string ContainsPattern(string value)
    {
        var escaped = value.Trim()
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_");
        return $"%{escaped}%";
    }

    private static AdminProjectAssignmentResult<T> Success<T>(T value)
        => new(value);

    private static AdminProjectAssignmentResult<T> Failure<T>(
        AdminProjectAssignmentError error,
        string code)
        => new(default, error, code);
}
