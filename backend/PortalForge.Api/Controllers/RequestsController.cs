using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalForge.Application.UseCases.Requests.Commands.ApproveRequestStep;
using PortalForge.Application.UseCases.Requests.Commands.SubmitRequest;
using PortalForge.Application.UseCases.Requests.Queries.GetMyRequests;
using PortalForge.Application.UseCases.Requests.Queries.GetRequestsToApprove;
using System.Security.Claims;

namespace PortalForge.Api.Controllers;

[ApiController]
[Route("api/requests")]
[Authorize]
public class RequestsController : ControllerBase
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
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized("User ID not found in token");
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
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized("User ID not found in token");
        }

        var query = new GetRequestsToApproveQuery { ApproverId = userGuid };
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
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized("User ID not found in token");
        }

        command.SubmittedById = userGuid;
        var result = await _mediator.Send(command);
        
        return CreatedAtAction(nameof(GetMyRequests), null, result);
    }

    /// <summary>
    /// Approve a request step
    /// </summary>
    [HttpPost("{requestId}/steps/{stepId}/approve")]
    [Authorize(Policy = "RequirePermission:requests.approve")]
    public async Task<ActionResult> ApproveStep(Guid requestId, Guid stepId, [FromBody] ApproveStepDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized("User ID not found in token");
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
}

public class ApproveStepDto
{
    public string? Comment { get; set; }
}

