using System.Net;
using System.Reflection;
using System.Security.Claims;
using CAPNEXAAPI.Areas.Admin.Controllers;
using CAPNEXAAPI.DTOs.Request;
using CAPNEXAAPI.DTOs.Response;
using CAPNEXAAPI.Services;
using CAPNEXAAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CAPNEXAAPI.Tests;

public sealed class AdminProjectAssignmentsControllerTests
{
    [Fact]
    public void Controller_RequiresAdminAreaRoleAndExpectedRoutes()
    {
        var type = typeof(ProjectAssignmentsController);

        var area = Assert.Single(type.GetCustomAttributes<AreaAttribute>());
        Assert.Equal(CD.ADMIN_AREA, area.RouteValue);

        var authorize = Assert.Single(type.GetCustomAttributes<AuthorizeAttribute>());
        Assert.Equal(CD.ADMIN_ROLE, authorize.Roles);
        Assert.Empty(type.GetCustomAttributes<AllowAnonymousAttribute>());

        var route = Assert.Single(type.GetCustomAttributes<RouteAttribute>());
        Assert.Equal("api/[area]/[controller]", route.Template);

        var unassigned = type.GetMethod(nameof(ProjectAssignmentsController.GetUnassignedProjects))!;
        Assert.Equal(
            "unassigned",
            unassigned.GetCustomAttribute<HttpGetAttribute>()!.Template);

        var assign = type.GetMethod(nameof(ProjectAssignmentsController.AssignSupervisor))!;
        Assert.Equal(
            "{projectId:int}",
            assign.GetCustomAttribute<HttpPutAttribute>()!.Template);

        foreach (var method in type.GetMethods(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            Assert.Empty(method.GetCustomAttributes(typeof(AllowAnonymousAttribute), false));
        }
    }

    [Fact]
    public async Task GetUnassignedProjects_ReturnsOkAndDelegatesQuery()
    {
        var value = new AdminUnassignedProjectListDto
        {
            Page = 2,
            PageSize = 5,
            TotalCount = 1,
            TotalPages = 1,
            Items = [new AdminUnassignedProjectListItemDto { ProjectId = 9 }]
        };
        var service = new FakeAdminProjectAssignmentService
        {
            ListResult = new AdminProjectAssignmentResult<AdminUnassignedProjectListDto>(value)
        };
        var controller = Controller(service);
        var query = new AdminUnassignedProjectsQueryDto { Page = 2, PageSize = 5 };

        var result = await controller.GetUnassignedProjects(query);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(value, ok.Value);
        Assert.Same(query, service.LastQuery);
    }

    [Fact]
    public async Task GetUnassignedProjects_InvalidPagination_ReturnsBadRequest()
    {
        var service = new FakeAdminProjectAssignmentService
        {
            ListResult = new AdminProjectAssignmentResult<AdminUnassignedProjectListDto>(
                default, AdminProjectAssignmentError.InvalidRequest, "invalid_pagination")
        };
        var controller = Controller(service);

        var result = await controller.GetUnassignedProjects(
            new AdminUnassignedProjectsQueryDto { Page = 0 });

        Assert.Equal(400, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
        Assert.Contains("invalid_pagination", BodyOf(result));
    }

    [Fact]
    public async Task AssignSupervisor_ReturnsOkAndPassesAdminIdRequestAndIp()
    {
        var value = new AdminProjectAssignmentDto { ProjectId = 9 };
        var service = new FakeAdminProjectAssignmentService
        {
            AssignResult = new AdminProjectAssignmentResult<AdminProjectAssignmentDto>(value)
        };
        var controller = Controller(service);
        var request = new AssignSupervisorDto { SupervisorId = 3 };

        var result = await controller.AssignSupervisor(9, request);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(value, ok.Value);
        Assert.NotNull(service.LastAssign);
        Assert.Equal("admin-1", service.LastAssign!.Value.AdminUserId);
        Assert.Equal(9, service.LastAssign.Value.ProjectId);
        Assert.Same(request, service.LastAssign.Value.Request);
        Assert.Equal("10.0.0.7", service.LastAssign.Value.IpAddress);
    }

    [Fact]
    public async Task AssignSupervisor_ProjectNotFound_ReturnsNotFound()
    {
        var service = new FakeAdminProjectAssignmentService
        {
            AssignResult = new AdminProjectAssignmentResult<AdminProjectAssignmentDto>(
                default, AdminProjectAssignmentError.NotFound, "project_not_found")
        };
        var controller = Controller(service);

        var result = await controller.AssignSupervisor(
            9, new AssignSupervisorDto { SupervisorId = 3 });

        Assert.Equal(404, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
        Assert.Contains("project_not_found", BodyOf(result));
    }

    [Fact]
    public async Task AssignSupervisor_ProjectNotApproved_ReturnsConflict()
    {
        var service = new FakeAdminProjectAssignmentService
        {
            AssignResult = new AdminProjectAssignmentResult<AdminProjectAssignmentDto>(
                default, AdminProjectAssignmentError.Conflict, "project_not_approved")
        };
        var controller = Controller(service);

        var result = await controller.AssignSupervisor(
            9, new AssignSupervisorDto { SupervisorId = 3 });

        Assert.Equal(409, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
        Assert.Contains("project_not_approved", BodyOf(result));
    }

    [Fact]
    public async Task AssignSupervisor_SupervisorNotFound_ReturnsNotFound()
    {
        var service = new FakeAdminProjectAssignmentService
        {
            AssignResult = new AdminProjectAssignmentResult<AdminProjectAssignmentDto>(
                default, AdminProjectAssignmentError.NotFound, "supervisor_not_found")
        };
        var controller = Controller(service);

        var result = await controller.AssignSupervisor(
            9, new AssignSupervisorDto { SupervisorId = 3 });

        Assert.Equal(404, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
        Assert.Contains("supervisor_not_found", BodyOf(result));
    }

    [Fact]
    public async Task AssignSupervisor_SupervisorInactive_ReturnsConflict()
    {
        var service = new FakeAdminProjectAssignmentService
        {
            AssignResult = new AdminProjectAssignmentResult<AdminProjectAssignmentDto>(
                default, AdminProjectAssignmentError.Conflict, "supervisor_inactive")
        };
        var controller = Controller(service);

        var result = await controller.AssignSupervisor(
            9, new AssignSupervisorDto { SupervisorId = 3 });

        Assert.Equal(409, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
        Assert.Contains("supervisor_inactive", BodyOf(result));
    }

    [Fact]
    public async Task AssignSupervisor_CapacityReached_ReturnsConflict()
    {
        var service = new FakeAdminProjectAssignmentService
        {
            AssignResult = new AdminProjectAssignmentResult<AdminProjectAssignmentDto>(
                default, AdminProjectAssignmentError.Conflict, "supervisor_capacity_reached")
        };
        var controller = Controller(service);

        var result = await controller.AssignSupervisor(
            9, new AssignSupervisorDto { SupervisorId = 3 });

        Assert.Equal(409, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
        Assert.Contains("supervisor_capacity_reached", BodyOf(result));
    }

    [Fact]
    public async Task AssignSupervisor_InvalidPayload_ReturnsBadRequest()
    {
        var service = new FakeAdminProjectAssignmentService
        {
            AssignResult = new AdminProjectAssignmentResult<AdminProjectAssignmentDto>(
                default, AdminProjectAssignmentError.InvalidRequest, "invalid_supervisor_payload")
        };
        var controller = Controller(service);

        var result = await controller.AssignSupervisor(9, new AssignSupervisorDto());

        Assert.Equal(400, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
        Assert.Contains("invalid_supervisor_payload", BodyOf(result));
    }

    [Fact]
    public async Task AssignSupervisor_OperationFailure_ReturnsServerError()
    {
        var service = new FakeAdminProjectAssignmentService
        {
            AssignResult = new AdminProjectAssignmentResult<AdminProjectAssignmentDto>(
                default,
                AdminProjectAssignmentError.OperationFailed,
                "admin_project_assignment_operation_failed")
        };
        var controller = Controller(service);

        var result = await controller.AssignSupervisor(
            9, new AssignSupervisorDto { SupervisorId = 3 });

        Assert.Equal(500, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
        Assert.Contains("admin_project_assignment_operation_failed", BodyOf(result));
    }

    private static ProjectAssignmentsController Controller(
        FakeAdminProjectAssignmentService service)
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, "admin-1")], "test"))
        };
        context.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.7");

        return new ProjectAssignmentsController(service)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
    }

    private static string BodyOf(IActionResult result)
        => Assert.IsAssignableFrom<ObjectResult>(result).Value?.ToString() ?? string.Empty;

    private sealed class FakeAdminProjectAssignmentService : IAdminProjectAssignmentService
    {
        public AdminProjectAssignmentResult<AdminUnassignedProjectListDto> ListResult { get; init; }
            = new(new AdminUnassignedProjectListDto());
        public AdminProjectAssignmentResult<AdminProjectAssignmentDto> AssignResult { get; init; }
            = new(new AdminProjectAssignmentDto());
        public AdminUnassignedProjectsQueryDto? LastQuery { get; private set; }
        public (string AdminUserId, int ProjectId, AssignSupervisorDto? Request, string? IpAddress)?
            LastAssign { get; private set; }

        public Task<AdminProjectAssignmentResult<AdminUnassignedProjectListDto>>
            GetUnassignedProjectsAsync(AdminUnassignedProjectsQueryDto query)
        {
            LastQuery = query;
            return Task.FromResult(ListResult);
        }

        public Task<AdminProjectAssignmentResult<AdminProjectAssignmentDto>>
            AssignSupervisorAsync(
                string adminUserId,
                int projectId,
                AssignSupervisorDto? request,
                string? ipAddress)
        {
            LastAssign = (adminUserId, projectId, request, ipAddress);
            return Task.FromResult(AssignResult);
        }
    }
}
