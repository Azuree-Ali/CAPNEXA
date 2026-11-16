using CAPNEXAAPI.DTOs.Request;
using CAPNEXAAPI.DTOs.Response;
using CAPNEXAAPI.Models;
using CAPNEXAAPI.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CAPNEXAAPI.Services;

public sealed class AdminProposalService : IAdminProposalService
{
    public const int MaximumPageSize = 100;

    private const int MaximumReasonLength = 2000;
    private const string LikeEscapeCharacter = "\\";

    private readonly IRepository<Project> _projects;
    private readonly IRepository<Team> _teams;
    private readonly IRepository<TeamMember> _members;
    private readonly IRepository<StudentProfile> _students;
    private readonly IRepository<ApplicationUser> _users;
    private readonly IRepository<AuditLog> _auditLogs;

    public AdminProposalService(
        IRepository<Project> projects,
        IRepository<Team> teams,
        IRepository<TeamMember> members,
        IRepository<StudentProfile> students,
        IRepository<ApplicationUser> users,
        IRepository<AuditLog> auditLogs)
    {
        _projects = projects;
        _teams = teams;
        _members = members;
        _students = students;
        _users = users;
        _auditLogs = auditLogs;
    }

    public async Task<AdminProposalResult<AdminProposalListDto>> GetProposalsAsync(
        AdminProposalQueryDto query)
    {
        if (query.Page < 1 || query.PageSize < 1 || query.PageSize > MaximumPageSize)
            return Failure<AdminProposalListDto>(
                AdminProposalError.InvalidRequest,
                "invalid_pagination");

        var status = string.IsNullOrWhiteSpace(query.Status)
            ? ProjectStatuses.PendingReview
            : query.Status.Trim();

        if (status != ProjectStatuses.PendingReview &&
            status != ProjectStatuses.Approved &&
            status != ProjectStatuses.Rejected)
            return Failure<AdminProposalListDto>(
                AdminProposalError.InvalidRequest,
                "invalid_status_filter");

        var projects = _projects.GetQueryable()
            .Where(project => project.Status == status);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = ContainsPattern(query.Search);
            projects = projects.Where(project =>
                EF.Functions.Like(project.Title, pattern, LikeEscapeCharacter) ||
                EF.Functions.Like(project.Team.Name, pattern, LikeEscapeCharacter) ||
                EF.Functions.Like(project.Team.TeamCode, pattern, LikeEscapeCharacter));
        }

        var totalCount = await projects.CountAsync();

        var ordered = status == ProjectStatuses.PendingReview
            ? projects
                .OrderBy(project => project.SubmittedAt)
                .ThenBy(project => project.Id)
            : projects
                .OrderByDescending(project => project.ReviewedAt)
                .ThenBy(project => project.Id);

        var rows = await ordered
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(project => new
            {
                ProjectId = project.Id,
                project.Title,
                project.Status,
                project.SubmittedAt,
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

        return Success(new AdminProposalListDto
        {
            Items = rows.Select(row => new AdminProposalListItemDto
            {
                ProjectId = row.ProjectId,
                Title = row.Title,
                Status = row.Status,
                SubmittedAt = row.SubmittedAt,
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

    public async Task<AdminProposalResult<AdminProposalDetailDto>> GetProposalAsync(int projectId)
    {
        var project = await _projects.GetOneAsync(
            item => item.Id == projectId && item.Status != ProjectStatuses.Draft,
            IsTracking: false);

        if (project is null)
            return Failure<AdminProposalDetailDto>(
                AdminProposalError.NotFound,
                "proposal_not_found");

        return Success(await BuildDetailAsync(project));
    }

    public async Task<AdminProposalResult<AdminProposalDetailDto>> ReviewProposalAsync(
        string adminUserId,
        int projectId,
        ReviewProjectProposalDto request,
        string? ipAddress)
    {
        var project = await _projects.GetOneAsync(item => item.Id == projectId);

        if (project is null || project.Status == ProjectStatuses.Draft)
            return Failure<AdminProposalDetailDto>(
                AdminProposalError.NotFound,
                "proposal_not_found");

        if (project.Status != ProjectStatuses.PendingReview)
            return Failure<AdminProposalDetailDto>(
                AdminProposalError.Conflict,
                "proposal_not_pending_review");

        var decision = request?.Decision?.Trim();
        if (decision != ProjectStatuses.Approved && decision != ProjectStatuses.Rejected)
            return Failure<AdminProposalDetailDto>(
                AdminProposalError.InvalidRequest,
                "invalid_decision");

        var reason = TrimToNull(request!.Reason);

        if (decision == ProjectStatuses.Rejected && reason is null)
            return Failure<AdminProposalDetailDto>(
                AdminProposalError.InvalidRequest,
                "rejection_reason_required");

        if (reason?.Length > MaximumReasonLength)
            return Failure<AdminProposalDetailDto>(
                AdminProposalError.InvalidRequest,
                "invalid_review_payload");

        project.Status = decision;
        project.ReviewReason = reason;
        project.ReviewedByUserId = adminUserId;
        project.ReviewedAt = DateTime.UtcNow;
        project.RejectionReason = decision == ProjectStatuses.Rejected ? reason : null;

        _projects.Update(project);
        await _auditLogs.CreateAsync(new AuditLog
        {
            UserId = adminUserId,
            IpAddress = ipAddress,
            Action = decision == ProjectStatuses.Approved
                ? "ProposalApproved"
                : "ProposalRejected",
            EntityName = "Project",
            EntityId = projectId.ToString(),
            Description = decision == ProjectStatuses.Approved
                ? "Project proposal approved."
                : "Project proposal rejected."
        });
        await _projects.CommitAsync();

        return Success(await BuildDetailAsync(project));
    }

    private async Task<AdminProposalDetailDto> BuildDetailAsync(Project project)
    {
        var team = await _teams.GetOneAsync(
            item => item.Id == project.TeamId,
            [item => item.Members],
            IsTracking: false);

        var members = team is null
            ? Array.Empty<TeamMember>()
            : team.Members.ToArray();

        var studentIds = members.Select(member => member.StudentId).ToArray();
        var students = await _students.GetAllAsync(
            student => studentIds.Contains(student.Id),
            [student => student.User],
            IsTracking: false);
        var studentsById = students.ToDictionary(student => student.Id);

        var reviewer = project.ReviewedByUserId is null
            ? null
            : await _users.GetOneAsync(
                user => user.Id == project.ReviewedByUserId,
                IsTracking: false);

        return new AdminProposalDetailDto
        {
            ProjectId = project.Id,
            Title = project.Title,
            Description = project.Description,
            Status = project.Status,
            CreatedAt = project.CreatedAt,
            SubmittedAt = project.SubmittedAt,
            Team = new AdminProposalTeamDto
            {
                TeamId = team?.Id ?? project.TeamId,
                Name = team?.Name ?? string.Empty,
                TeamCode = team?.TeamCode ?? string.Empty,
                MaxMembers = team?.MaxMembers ?? 0,
                MemberCount = members.Length,
                Members = members
                    .OrderByDescending(member => member.IsLeader)
                    .ThenBy(member => member.JoinedAt)
                    .Select(member =>
                    {
                        var student = studentsById[member.StudentId];
                        return new AdminProposalMemberDto
                        {
                            UserId = student.UserId,
                            FirstName = student.User.FirstName,
                            LastName = student.User.LastName,
                            Email = student.User.Email,
                            StudentNumber = student.StudentNumber,
                            Department = student.Department,
                            AcademicYear = student.AcademicYear,
                            IsLeader = member.IsLeader,
                            JoinedAt = member.JoinedAt
                        };
                    })
                    .ToArray()
            },
            Review = project.Status == ProjectStatuses.PendingReview
                ? null
                : new AdminProposalReviewDto
                {
                    Decision = project.Status,
                    Reason = project.ReviewReason,
                    ReviewedByUserId = project.ReviewedByUserId,
                    ReviewedByName = reviewer is null
                        ? null
                        : $"{reviewer.FirstName} {reviewer.LastName}".Trim(),
                    ReviewedAt = project.ReviewedAt
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

    private static string? TrimToNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        return trimmed.Length == 0 ? null : trimmed;
    }

    private static AdminProposalResult<T> Success<T>(T value)
        => new(value);

    private static AdminProposalResult<T> Failure<T>(AdminProposalError error, string code)
        => new(default, error, code);
}
