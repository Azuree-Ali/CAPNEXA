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

public sealed class AdminProposalsControllerTests
{
    [Fact]
    public void Controller_RequiresAdminAreaRoleAndExpectedRoutes()
    {
        var type = typeof(ProjectProposalsController);

        var area = Assert.Single(type.GetCustomAttributes<AreaAttribute>());
        Assert.Equal(CD.ADMIN_AREA, area.RouteValue);

        var authorize = Assert.Single(type.GetCustomAttributes<AuthorizeAttribute>());
        Assert.Equal(CD.ADMIN_ROLE, authorize.Roles);
        Assert.Empty(type.GetCustomAttributes<AllowAnonymousAttribute>());

        var route = Assert.Single(type.GetCustomAttributes<RouteAttribute>());
        Assert.Equal("api/[area]/[controller]", route.Template);

        var list = type.GetMethod(nameof(ProjectProposalsController.GetProposals))!;
        Assert.NotNull(list.GetCustomAttribute<HttpGetAttribute>());
        Assert.Null(list.GetCustomAttribute<HttpGetAttribute>()!.Template);

        var detail = type.GetMethod(nameof(ProjectProposalsController.GetProposal))!;
        Assert.Equal(
            "{projectId:int}",
            detail.GetCustomAttribute<HttpGetAttribute>()!.Template);

        var review = type.GetMethod(nameof(ProjectProposalsController.ReviewProposal))!;
        Assert.Equal(
            "{projectId:int}/review",
            review.GetCustomAttribute<HttpPutAttribute>()!.Template);

        foreach (var method in type.GetMethods(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            Assert.Empty(method.GetCustomAttributes(typeof(AllowAnonymousAttribute), false));
        }
    }

    [Fact]
    public async Task GetProposals_ReturnsOkAndDelegatesQueryToService()
    {
        var value = new AdminProposalListDto
        {
            Page = 2,
            PageSize = 10,
            TotalCount = 1,
            TotalPages = 1,
            Items = [new AdminProposalListItemDto { ProjectId = 7 }]
        };
        var service = new FakeAdminProposalService
        {
            ListResult = new AdminProposalResult<AdminProposalListDto>(value)
        };
        var controller = Controller(service);
        var query = new AdminProposalQueryDto { Page = 2, PageSize = 10 };

        var result = await controller.GetProposals(query);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(value, ok.Value);
        Assert.Same(query, service.LastQuery);
    }

    [Fact]
    public async Task GetProposals_InvalidStatus_ReturnsBadRequestBusinessError()
    {
        var service = new FakeAdminProposalService
        {
            ListResult = new AdminProposalResult<AdminProposalListDto>(
                default, AdminProposalError.InvalidRequest, "invalid_status_filter")
        };
        var controller = Controller(service);

        var result = await controller.GetProposals(
            new AdminProposalQueryDto { Status = "Draft" });

        Assert.Equal(400, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
        Assert.Contains("invalid_status_filter", BodyOf(result));
    }

    [Fact]
    public async Task GetProposal_ReturnsOkAndDelegatesProjectId()
    {
        var value = new AdminProposalDetailDto { ProjectId = 7, Status = "PendingReview" };
        var service = new FakeAdminProposalService
        {
            DetailResult = new AdminProposalResult<AdminProposalDetailDto>(value)
        };
        var controller = Controller(service);

        var result = await controller.GetProposal(7);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(value, ok.Value);
        Assert.Equal(7, service.LastDetailProjectId);
    }

    [Fact]
    public async Task GetProposal_MissingOrDraft_ReturnsNotFound()
    {
        var service = new FakeAdminProposalService
        {
            DetailResult = new AdminProposalResult<AdminProposalDetailDto>(
                default, AdminProposalError.NotFound, "proposal_not_found")
        };
        var controller = Controller(service);

        var result = await controller.GetProposal(404);

        Assert.Equal(404, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
        Assert.Contains("proposal_not_found", BodyOf(result));
    }

    [Fact]
    public async Task ReviewProposal_ReturnsOkAndPassesAdminIdProjectBodyAndIp()
    {
        var value = new AdminProposalDetailDto { ProjectId = 7, Status = "Approved" };
        var service = new FakeAdminProposalService
        {
            ReviewResult = new AdminProposalResult<AdminProposalDetailDto>(value)
        };
        var controller = Controller(service);
        var request = new ReviewProjectProposalDto
        {
            Decision = "Approved",
            Reason = "Meets the requirements"
        };

        var result = await controller.ReviewProposal(7, request);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(value, ok.Value);
        Assert.NotNull(service.LastReview);
        Assert.Equal("admin-1", service.LastReview.Value.AdminUserId);
        Assert.Equal(7, service.LastReview.Value.ProjectId);
        Assert.Same(request, service.LastReview.Value.Request);
        Assert.Equal("10.0.0.7", service.LastReview.Value.IpAddress);
    }

    [Fact]
    public async Task ReviewProposal_Conflict_ReturnsConflictBusinessError()
    {
        var service = new FakeAdminProposalService
        {
            ReviewResult = new AdminProposalResult<AdminProposalDetailDto>(
                default, AdminProposalError.Conflict, "proposal_not_pending_review")
        };
        var controller = Controller(service);

        var result = await controller.ReviewProposal(
            7, new ReviewProjectProposalDto { Decision = "Approved" });

        Assert.Equal(409, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
        Assert.Contains("proposal_not_pending_review", BodyOf(result));
    }

    [Fact]
    public async Task ReviewProposal_InvalidDecision_ReturnsBadRequest()
    {
        var service = new FakeAdminProposalService
        {
            ReviewResult = new AdminProposalResult<AdminProposalDetailDto>(
                default, AdminProposalError.InvalidRequest, "invalid_decision")
        };
        var controller = Controller(service);

        var result = await controller.ReviewProposal(
            7, new ReviewProjectProposalDto { Decision = "Maybe" });

        Assert.Equal(400, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
        Assert.Contains("invalid_decision", BodyOf(result));
    }

    [Fact]
    public async Task ReviewProposal_MissingRejectionReason_ReturnsBadRequest()
    {
        var service = new FakeAdminProposalService
        {
            ReviewResult = new AdminProposalResult<AdminProposalDetailDto>(
                default, AdminProposalError.InvalidRequest, "rejection_reason_required")
        };
        var controller = Controller(service);

        var result = await controller.ReviewProposal(
            7, new ReviewProjectProposalDto { Decision = "Rejected" });

        Assert.Equal(400, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
        Assert.Contains("rejection_reason_required", BodyOf(result));
    }

    [Fact]
    public async Task ReviewProposal_OperationFailure_ReturnsServerError()
    {
        var service = new FakeAdminProposalService
        {
            ReviewResult = new AdminProposalResult<AdminProposalDetailDto>(
                default, AdminProposalError.OperationFailed, "admin_proposal_operation_failed")
        };
        var controller = Controller(service);

        var result = await controller.ReviewProposal(
            7, new ReviewProjectProposalDto { Decision = "Approved" });

        Assert.Equal(500, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
    }

    private static ProjectProposalsController Controller(FakeAdminProposalService service)
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, "admin-1")], "test"))
        };
        context.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.7");

        return new ProjectProposalsController(service)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
    }

    private static string BodyOf(IActionResult result)
        => Assert.IsAssignableFrom<ObjectResult>(result).Value?.ToString() ?? string.Empty;

    private sealed class FakeAdminProposalService : IAdminProposalService
    {
        public AdminProposalResult<AdminProposalListDto> ListResult { get; init; }
            = new(new AdminProposalListDto());
        public AdminProposalResult<AdminProposalDetailDto> DetailResult { get; init; }
            = new(new AdminProposalDetailDto());
        public AdminProposalResult<AdminProposalDetailDto> ReviewResult { get; init; }
            = new(new AdminProposalDetailDto());
        public AdminProposalQueryDto? LastQuery { get; private set; }
        public int? LastDetailProjectId { get; private set; }
        public (string AdminUserId, int ProjectId, ReviewProjectProposalDto Request, string? IpAddress)?
            LastReview { get; private set; }

        public Task<AdminProposalResult<AdminProposalListDto>> GetProposalsAsync(
            AdminProposalQueryDto query)
        {
            LastQuery = query;
            return Task.FromResult(ListResult);
        }

        public Task<AdminProposalResult<AdminProposalDetailDto>> GetProposalAsync(int projectId)
        {
            LastDetailProjectId = projectId;
            return Task.FromResult(DetailResult);
        }

        public Task<AdminProposalResult<AdminProposalDetailDto>> ReviewProposalAsync(
            string adminUserId,
            int projectId,
            ReviewProjectProposalDto request,
            string? ipAddress)
        {
            LastReview = (adminUserId, projectId, request, ipAddress);
            return Task.FromResult(ReviewResult);
        }
    }
}
