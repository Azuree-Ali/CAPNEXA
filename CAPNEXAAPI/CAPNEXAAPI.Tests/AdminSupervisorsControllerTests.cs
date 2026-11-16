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

public sealed class AdminSupervisorsControllerTests
{
    [Fact]
    public void Controller_RequiresAdminAreaRoleAndExpectedRoutes()
    {
        var type = typeof(SupervisorsController);

        var area = Assert.Single(type.GetCustomAttributes<AreaAttribute>());
        Assert.Equal(CD.ADMIN_AREA, area.RouteValue);

        var authorize = Assert.Single(type.GetCustomAttributes<AuthorizeAttribute>());
        Assert.Equal(CD.ADMIN_ROLE, authorize.Roles);
        Assert.Empty(type.GetCustomAttributes<AllowAnonymousAttribute>());

        var route = Assert.Single(type.GetCustomAttributes<RouteAttribute>());
        Assert.Equal("api/[area]/[controller]", route.Template);

        var list = type.GetMethod(nameof(SupervisorsController.GetSupervisors))!;
        var listAttribute = list.GetCustomAttribute<HttpGetAttribute>();
        Assert.NotNull(listAttribute);
        Assert.Null(listAttribute!.Template);

        var detail = type.GetMethod(nameof(SupervisorsController.GetSupervisor))!;
        Assert.Equal(
            "{supervisorId:int}",
            detail.GetCustomAttribute<HttpGetAttribute>()!.Template);

        var update = type.GetMethod(nameof(SupervisorsController.UpdateMaxProjects))!;
        Assert.Equal(
            "{supervisorId:int}/max-projects",
            update.GetCustomAttribute<HttpPutAttribute>()!.Template);

        foreach (var method in type.GetMethods(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            Assert.Empty(method.GetCustomAttributes(typeof(AllowAnonymousAttribute), false));
        }
    }

    [Fact]
    public async Task GetSupervisors_ReturnsOkAndDelegatesQuery()
    {
        var value = new AdminSupervisorListDto
        {
            Page = 2,
            PageSize = 10,
            TotalCount = 1,
            TotalPages = 1,
            Items = [new AdminSupervisorListItemDto { SupervisorId = 3 }]
        };
        var service = new FakeAdminSupervisorService
        {
            ListResult = new AdminSupervisorResult<AdminSupervisorListDto>(value)
        };
        var controller = Controller(service);
        var query = new AdminSupervisorQueryDto { Page = 2, PageSize = 10 };

        var result = await controller.GetSupervisors(query);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(value, ok.Value);
        Assert.Same(query, service.LastQuery);
    }

    [Fact]
    public async Task GetSupervisors_InvalidPagination_ReturnsBadRequest()
    {
        var service = new FakeAdminSupervisorService
        {
            ListResult = new AdminSupervisorResult<AdminSupervisorListDto>(
                default, AdminSupervisorError.InvalidRequest, "invalid_pagination")
        };
        var controller = Controller(service);

        var result = await controller.GetSupervisors(
            new AdminSupervisorQueryDto { Page = 0 });

        Assert.Equal(400, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
        Assert.Contains("invalid_pagination", BodyOf(result));
    }

    [Fact]
    public async Task GetSupervisor_ReturnsOkAndDelegatesId()
    {
        var value = new AdminSupervisorDetailDto { SupervisorId = 4 };
        var service = new FakeAdminSupervisorService
        {
            DetailResult = new AdminSupervisorResult<AdminSupervisorDetailDto>(value)
        };
        var controller = Controller(service);

        var result = await controller.GetSupervisor(4);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(value, ok.Value);
        Assert.Equal(4, service.LastDetailSupervisorId);
    }

    [Fact]
    public async Task GetSupervisor_Missing_ReturnsNotFound()
    {
        var service = new FakeAdminSupervisorService
        {
            DetailResult = new AdminSupervisorResult<AdminSupervisorDetailDto>(
                default, AdminSupervisorError.NotFound, "supervisor_not_found")
        };
        var controller = Controller(service);

        var result = await controller.GetSupervisor(4);

        Assert.Equal(404, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
        Assert.Contains("supervisor_not_found", BodyOf(result));
    }

    [Fact]
    public async Task UpdateMaxProjects_ReturnsOkAndPassesAdminIdRequestAndIp()
    {
        var value = new AdminSupervisorDetailDto { SupervisorId = 4, MaxProjects = 3 };
        var service = new FakeAdminSupervisorService
        {
            UpdateResult = new AdminSupervisorResult<AdminSupervisorDetailDto>(value)
        };
        var controller = Controller(service);
        var request = new UpdateSupervisorCapacityDto { MaxProjects = 3 };

        var result = await controller.UpdateMaxProjects(4, request);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(value, ok.Value);
        Assert.NotNull(service.LastUpdate);
        Assert.Equal("admin-1", service.LastUpdate!.Value.AdminUserId);
        Assert.Equal(4, service.LastUpdate.Value.SupervisorId);
        Assert.Same(request, service.LastUpdate.Value.Request);
        Assert.Equal("10.0.0.7", service.LastUpdate.Value.IpAddress);
    }

    [Fact]
    public async Task UpdateMaxProjects_BelowAssignedProjects_ReturnsConflict()
    {
        var service = new FakeAdminSupervisorService
        {
            UpdateResult = new AdminSupervisorResult<AdminSupervisorDetailDto>(
                default,
                AdminSupervisorError.Conflict,
                "capacity_below_assigned_projects")
        };
        var controller = Controller(service);

        var result = await controller.UpdateMaxProjects(
            4, new UpdateSupervisorCapacityDto { MaxProjects = 1 });

        Assert.Equal(409, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
        Assert.Contains("capacity_below_assigned_projects", BodyOf(result));
    }

    [Fact]
    public async Task UpdateMaxProjects_InvalidPayload_ReturnsBadRequest()
    {
        var service = new FakeAdminSupervisorService
        {
            UpdateResult = new AdminSupervisorResult<AdminSupervisorDetailDto>(
                default, AdminSupervisorError.InvalidRequest, "invalid_capacity_payload")
        };
        var controller = Controller(service);

        var result = await controller.UpdateMaxProjects(4, new UpdateSupervisorCapacityDto());

        Assert.Equal(400, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
        Assert.Contains("invalid_capacity_payload", BodyOf(result));
    }

    [Fact]
    public async Task UpdateMaxProjects_OperationFailure_ReturnsServerError()
    {
        var service = new FakeAdminSupervisorService
        {
            UpdateResult = new AdminSupervisorResult<AdminSupervisorDetailDto>(
                default,
                AdminSupervisorError.OperationFailed,
                "admin_supervisor_operation_failed")
        };
        var controller = Controller(service);

        var result = await controller.UpdateMaxProjects(
            4, new UpdateSupervisorCapacityDto { MaxProjects = 4 });

        Assert.Equal(500, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
        Assert.Contains("admin_supervisor_operation_failed", BodyOf(result));
    }

    private static SupervisorsController Controller(FakeAdminSupervisorService service)
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, "admin-1")], "test"))
        };
        context.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.7");

        return new SupervisorsController(service)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
    }

    private static string BodyOf(IActionResult result)
        => Assert.IsAssignableFrom<ObjectResult>(result).Value?.ToString() ?? string.Empty;

    private sealed class FakeAdminSupervisorService : IAdminSupervisorService
    {
        public AdminSupervisorResult<AdminSupervisorListDto> ListResult { get; init; }
            = new(new AdminSupervisorListDto());
        public AdminSupervisorResult<AdminSupervisorDetailDto> DetailResult { get; init; }
            = new(new AdminSupervisorDetailDto());
        public AdminSupervisorResult<AdminSupervisorDetailDto> UpdateResult { get; init; }
            = new(new AdminSupervisorDetailDto());
        public AdminSupervisorQueryDto? LastQuery { get; private set; }
        public int? LastDetailSupervisorId { get; private set; }
        public (string AdminUserId, int SupervisorId, UpdateSupervisorCapacityDto? Request, string? IpAddress)?
            LastUpdate { get; private set; }

        public Task<AdminSupervisorResult<AdminSupervisorListDto>> GetSupervisorsAsync(
            AdminSupervisorQueryDto query)
        {
            LastQuery = query;
            return Task.FromResult(ListResult);
        }

        public Task<AdminSupervisorResult<AdminSupervisorDetailDto>> GetSupervisorAsync(
            int supervisorId)
        {
            LastDetailSupervisorId = supervisorId;
            return Task.FromResult(DetailResult);
        }

        public Task<AdminSupervisorResult<AdminSupervisorDetailDto>> UpdateMaxProjectsAsync(
            string adminUserId,
            int supervisorId,
            UpdateSupervisorCapacityDto? request,
            string? ipAddress)
        {
            LastUpdate = (adminUserId, supervisorId, request, ipAddress);
            return Task.FromResult(UpdateResult);
        }
    }
}
