using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalForge.Api.DTOs.Requests.Requests;
using PortalForge.Application.UseCases.Requests.Commands.ApproveRequestStep;
using PortalForge.Application.UseCases.Requests.Commands.EditRequest;
using PortalForge.Application.UseCases.Requests.Commands.RejectRequestStep;
using PortalForge.Application.UseCases.Requests.Commands.SubmitRequest;
using PortalForge.Application.UseCases.Requests.Queries.GetMyRequests;
using PortalForge.Application.UseCases.Requests.Queries.GetPendingApprovals;
using PortalForge.Application.UseCases.Requests.Queries.GetRequestsToApprove;

namespace PortalForge.Api.Controllers;

[Route("api/requests")]
[Authorize]
public class RequestsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly ILogger<RequestsController> _logger;

    public RequestsController(IMediator mediator, ILogger<RequestsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get current user's requests
    /// </summary>
    [HttpGet("my-requests")]
    [Authorize(Policy = "RequirePermission:requests.view")]
    public async Task<ActionResult> GetMyRequests()
    {
        var unauthorizedResult = GetUserIdOrUnauthorized(out var userGuid);
        if (unauthorizedResult != null)
        {
            return unauthorizedResult;
        }

        var query = new GetMyRequestsQuery { UserId = userGuid };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get requests pending approval by current user
    /// </summary>
    [HttpGet("to-approve")]
    [Authorize(Policy = "RequirePermission:requests.approve")]
    public async Task<ActionResult> GetRequestsToApprove()
    {
        var unauthorizedResult = GetUserIdOrUnauthorized(out var userGuid);
        if (unauthorizedResult != null)
        {
            return unauthorizedResult;
        }

        var query = new GetRequestsToApproveQuery { ApproverId = userGuid };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get pending approvals (requests awaiting current user's approval)
    /// </summary>
    [HttpGet("pending-approvals")]
    [Authorize(Policy = "RequirePermission:requests.approve")]
    public async Task<ActionResult> GetPendingApprovals()
    {
        var unauthorizedResult = GetUserIdOrUnauthorized(out var userGuid);
        if (unauthorizedResult != null)
        {
            return unauthorizedResult;
        }

        var query = new GetPendingApprovalsQuery { UserId = userGuid };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Submit a new request
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequirePermission:requests.create")]
    public async Task<ActionResult> SubmitRequest([FromBody] SubmitRequestCommand command)
    {
        var unauthorizedResult = GetUserIdOrUnauthorized(out var userGuid);
        if (unauthorizedResult != null)
        {
            return unauthorizedResult;
        }

        command.SubmittedById = userGuid;
        var result = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetMyRequests), null, result);
    }

    /// <summary>
    /// Edit an existing request (Draft or InReview only)
    /// </summary>
    [HttpPut("{requestId:guid}")]
    [Authorize(Policy = "RequirePermission:requests.edit")]
    public async Task<IActionResult> EditRequest(
        Guid requestId,
        [FromBody] EditRequestRequest request)
    {
        var unauthorizedResult = GetUserIdOrUnauthorized(out var userGuid);
        if (unauthorizedResult != null)
        {
            return unauthorizedResult;
        }

        var command = new EditRequestCommand
        {
            RequestId = requestId,
            EditedByUserId = userGuid,
            NewFormData = request.FormData,
            ChangeReason = request.ChangeReason
        };

        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Approve a request step
    /// </summary>
    [HttpPost("{requestId}/steps/{stepId}/approve")]
    [Authorize(Policy = "RequirePermission:requests.approve")]
    public async Task<ActionResult> ApproveStep(Guid requestId, Guid stepId, [FromBody] ApproveStepDto dto)
    {
        var unauthorizedResult = GetUserIdOrUnauthorized(out var userGuid);
        if (unauthorizedResult != null)
        {
            return unauthorizedResult;
        }

        var command = new ApproveRequestStepCommand
        {
            RequestId = requestId,
            StepId = stepId,
            ApproverId = userGuid,
            Comment = dto.Comment
        };

        var result = await _mediator.Send(command);
        
        if (!result.Success)
        {
            return BadRequest(result.Message);
        }

        return Ok(result);
    }

    /// <summary>
    /// Reject a request step
    /// </summary>
    [HttpPost("{requestId}/steps/{stepId}/reject")]
    [Authorize(Policy = "RequirePermission:requests.approve")]
    public async Task<ActionResult> RejectStep(Guid requestId, Guid stepId, [FromBody] RejectStepDto dto)
    {
        var unauthorizedResult = GetUserIdOrUnauthorized(out var userGuid);
        if (unauthorizedResult != null)
        {
            return unauthorizedResult;
        }

        var command = new RejectRequestStepCommand
        {
            RequestId = requestId,
            StepId = stepId,
            ApproverId = userGuid,
            Reason = dto.Reason
        };

        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return BadRequest(result.Message);
        }

        return Ok(result);
    }
}

