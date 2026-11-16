using CAPNEXAAPI.DTOs.Request;
using CAPNEXAAPI.DTOs.Response;
using CAPNEXAAPI.Models;
using CAPNEXAAPI.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CAPNEXAAPI.Services;

public sealed class AdminSupervisorService : IAdminSupervisorService
{
    public const int MaximumPageSize = 100;
    public const int MaximumProjects = 20;

    private const string LikeEscapeCharacter = "\\";

    private readonly IRepository<SupervisorProfile> _supervisors;
    private readonly IRepository<Project> _projects;
    private readonly IRepository<AuditLog> _auditLogs;

    public AdminSupervisorService(
        IRepository<SupervisorProfile> supervisors,
        IRepository<Project> projects,
        IRepository<AuditLog> auditLogs)
    {
        _supervisors = supervisors;
        _projects = projects;
        _auditLogs = auditLogs;
    }

    public async Task<AdminSupervisorResult<AdminSupervisorListDto>> GetSupervisorsAsync(
        AdminSupervisorQueryDto query)
    {
        if (query.Page < 1 || query.PageSize < 1 || query.PageSize > MaximumPageSize)
            return Failure<AdminSupervisorListDto>(
                AdminSupervisorError.InvalidRequest,
                "invalid_pagination");

        var supervisors = _supervisors.GetQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = ContainsPattern(query.Search);
            supervisors = supervisors.Where(supervisor =>
                EF.Functions.Like(supervisor.User.FirstName, pattern, LikeEscapeCharacter) ||
                EF.Functions.Like(supervisor.User.LastName, pattern, LikeEscapeCharacter) ||
                (supervisor.User.Email != null &&
                 EF.Functions.Like(supervisor.User.Email, pattern, LikeEscapeCharacter)) ||
                (supervisor.Department != null &&
                 EF.Functions.Like(supervisor.Department, pattern, LikeEscapeCharacter)) ||
                (supervisor.Specialization != null &&
                 EF.Functions.Like(supervisor.Specialization, pattern, LikeEscapeCharacter)));
        }

        if (!string.IsNullOrWhiteSpace(query.Department))
        {
            var department = query.Department.Trim();
            supervisors = supervisors.Where(supervisor =>
                supervisor.Department == department);
        }

        if (query.IsActive.HasValue)
            supervisors = supervisors.Where(supervisor =>
                supervisor.User.IsActive == query.IsActive.Value);

        if (query.HasCapacity.HasValue)
        {
            supervisors = query.HasCapacity.Value
                ? supervisors.Where(supervisor =>
                    supervisor.Projects.Count < supervisor.MaxProjects)
                : supervisors.Where(supervisor =>
                    supervisor.Projects.Count >= supervisor.MaxProjects);
        }

        var totalCount = await supervisors.CountAsync();

        var rows = await supervisors
            .OrderBy(supervisor => supervisor.User.LastName)
            .ThenBy(supervisor => supervisor.User.FirstName)
            .ThenBy(supervisor => supervisor.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(supervisor => new
            {
                SupervisorId = supervisor.Id,
                supervisor.UserId,
                FirstName = supervisor.User.FirstName,
                LastName = supervisor.User.LastName,
                Email = supervisor.User.Email,
                IsActive = supervisor.User.IsActive,
                supervisor.Department,
                supervisor.Specialization,
                supervisor.ResearchInterests,
                supervisor.MaxProjects,
                AssignedProjectsCount = supervisor.Projects.Count
            })
            .ToListAsync();

        return Success(new AdminSupervisorListDto
        {
            Items = rows.Select(row => new AdminSupervisorListItemDto
            {
                SupervisorId = row.SupervisorId,
                UserId = row.UserId,
                FirstName = row.FirstName,
                LastName = row.LastName,
                Email = row.Email,
                IsActive = row.IsActive,
                Department = row.Department,
                Specialization = row.Specialization,
                ResearchInterests = row.ResearchInterests,
                MaxProjects = row.MaxProjects,
                AssignedProjectsCount = row.AssignedProjectsCount,
                AvailableCapacity = Math.Max(0, row.MaxProjects - row.AssignedProjectsCount)
            }).ToArray(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        });
    }

    public async Task<AdminSupervisorResult<AdminSupervisorDetailDto>> GetSupervisorAsync(
        int supervisorId)
    {
        var supervisor = await _supervisors.GetOneAsync(
            item => item.Id == supervisorId,
            [item => item.User],
            IsTracking: false);

        if (supervisor is null)
            return Failure<AdminSupervisorDetailDto>(
                AdminSupervisorError.NotFound,
                "supervisor_not_found");

        return Success(await BuildDetailAsync(supervisor));
    }

    public async Task<AdminSupervisorResult<AdminSupervisorDetailDto>> UpdateMaxProjectsAsync(
        string adminUserId,
        int supervisorId,
        UpdateSupervisorCapacityDto? request,
        string? ipAddress)
    {
        var supervisor = await _supervisors.GetOneAsync(
            item => item.Id == supervisorId,
            [item => item.User]);

        if (supervisor is null)
            return Failure<AdminSupervisorDetailDto>(
                AdminSupervisorError.NotFound,
                "supervisor_not_found");

        if (request?.MaxProjects is null ||
            request.MaxProjects < 0 ||
            request.MaxProjects > MaximumProjects)
            return Failure<AdminSupervisorDetailDto>(
                AdminSupervisorError.InvalidRequest,
                "invalid_capacity_payload");

        var maxProjects = request.MaxProjects.Value;
        var assignedProjectsCount = await _projects.GetQueryable()
            .CountAsync(project => project.SupervisorId == supervisor.Id);

        if (maxProjects < assignedProjectsCount)
            return Failure<AdminSupervisorDetailDto>(
                AdminSupervisorError.Conflict,
                "capacity_below_assigned_projects");

        if (maxProjects == supervisor.MaxProjects)
            return Success(await BuildDetailAsync(supervisor));

        supervisor.MaxProjects = maxProjects;
        _supervisors.Update(supervisor);
        await _auditLogs.CreateAsync(new AuditLog
        {
            UserId = adminUserId,
            IpAddress = ipAddress,
            Action = "SupervisorCapacityUpdated",
            EntityName = "SupervisorProfile",
            EntityId = supervisor.Id.ToString(),
            Description = "Supervisor maximum project capacity updated."
        });
        await _supervisors.CommitAsync();

        return Success(await BuildDetailAsync(supervisor));
    }

    private async Task<AdminSupervisorDetailDto> BuildDetailAsync(SupervisorProfile supervisor)
    {
        var assignedProjects = await _projects.GetQueryable()
            .Where(project => project.SupervisorId == supervisor.Id)
            .OrderBy(project => project.Id)
            .Select(project => new AdminSupervisorProjectDto
            {
                ProjectId = project.Id,
                Title = project.Title,
                Status = project.Status,
                TeamId = project.TeamId,
                TeamName = project.Team.Name,
                TeamCode = project.Team.TeamCode
            })
            .ToListAsync();

        return new AdminSupervisorDetailDto
        {
            SupervisorId = supervisor.Id,
            UserId = supervisor.UserId,
            FirstName = supervisor.User.FirstName,
            LastName = supervisor.User.LastName,
            Email = supervisor.User.Email,
            IsActive = supervisor.User.IsActive,
            Department = supervisor.Department,
            Specialization = supervisor.Specialization,
            ResearchInterests = supervisor.ResearchInterests,
            MaxProjects = supervisor.MaxProjects,
            AssignedProjectsCount = assignedProjects.Count,
            AvailableCapacity = Math.Max(0, supervisor.MaxProjects - assignedProjects.Count),
            AssignedProjects = assignedProjects
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

    private static AdminSupervisorResult<T> Success<T>(T value)
        => new(value);

    private static AdminSupervisorResult<T> Failure<T>(
        AdminSupervisorError error,
        string code)
        => new(default, error, code);
}
