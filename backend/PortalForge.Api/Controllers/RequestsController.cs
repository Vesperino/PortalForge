using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalForge.Api.DTOs.Requests.Requests;
using PortalForge.Application.UseCases.Requests.Commands.AddRequestComment;
using PortalForge.Application.UseCases.Requests.Commands.ApproveRequestStep;
using PortalForge.Application.UseCases.Requests.Commands.EditRequest;
using PortalForge.Application.UseCases.Requests.Commands.RejectRequestStep;
using PortalForge.Application.UseCases.Requests.Commands.SubmitRequest;
using PortalForge.Application.UseCases.Requests.Commands.SubmitQuizAnswers;
using PortalForge.Application.UseCases.Requests.Queries.GetMyRequests;
using PortalForge.Application.UseCases.Requests.Queries.GetPendingApprovals;
using PortalForge.Application.UseCases.Requests.Queries.GetRequestById;
using PortalForge.Application.UseCases.Requests.Queries.GetRequestsToApprove;
using PortalForge.Application.UseCases.Requests.Queries.GetApprovalsHistory;
using PortalForge.Application.Common.Interfaces;
using System.Text.Json;

namespace PortalForge.Api.Controllers;

[Route("api/requests")]
[Authorize]
public class RequestsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly ILogger<RequestsController> _logger;
    private readonly IFileStorageService _fileStorageService;

    public RequestsController(
        IMediator mediator,
        ILogger<RequestsController> logger,
        IFileStorageService fileStorageService)
    {
        _mediator = mediator;
        _logger = logger;
        _fileStorageService = fileStorageService;
    }

    /// <summary>
    /// Get current user's requests
    /// All authenticated users can view their own requests
    /// </summary>
    [HttpGet("my-requests")]
    [Authorize]
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
    /// Get approvals history (requests approved/rejected by current user)
    /// </summary>
    [HttpGet("approvals-history")]
    [Authorize(Policy = "RequirePermission:requests.approve")]
    public async Task<ActionResult> GetApprovalsHistory()
    {
        var unauthorizedResult = GetUserIdOrUnauthorized(out var userGuid);
        if (unauthorizedResult != null)
        {
            return unauthorizedResult;
        }

        var query = new GetApprovalsHistoryQuery { UserId = userGuid };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get request details by ID including comments and edit history
    /// All authenticated users can view requests (handler checks ownership/approval rights)
    /// </summary>
    [HttpGet("{requestId:guid}")]
    [Authorize]
    public async Task<ActionResult> GetRequestById(Guid requestId)
    {
        var query = new GetRequestByIdQuery { RequestId = requestId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Submit a new request
    /// All authenticated users can submit requests - this is a basic employee function
    /// </summary>
    [HttpPost]
    [Authorize]
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
    /// Add a comment to a request
    /// All authenticated users can add comments (handler checks ownership/approval rights)
    /// Supports optional file attachments via FormData
    /// </summary>
    [HttpPost("{requestId:guid}/comments")]
    [Authorize]
    public async Task<ActionResult<Guid>> AddComment(
        Guid requestId,
        [FromForm] string? comment,
        [FromForm] List<IFormFile>? attachments)
    {
        var unauthorizedResult = GetUserIdOrUnauthorized(out var userGuid);
        if (unauthorizedResult != null)
        {
            return unauthorizedResult;
        }

        // Upload attachments if any
        List<string>? attachmentUrls = null;
        if (attachments != null && attachments.Count > 0)
        {
            attachmentUrls = new List<string>();
            foreach (var file in attachments)
            {
                var relativePath = await _fileStorageService.SaveFileAsync(
                    file.OpenReadStream(),
                    file.FileName,
                    "request-comments");
                attachmentUrls.Add(relativePath);
            }
        }

        var command = new AddRequestCommentCommand
        {
            RequestId = requestId,
            UserId = userGuid,
            Comment = comment ?? string.Empty,
            Attachments = attachmentUrls != null && attachmentUrls.Count > 0
                ? JsonSerializer.Serialize(attachmentUrls)
                : null
        };

        var commentId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetMyRequests), null, commentId);
    }

    /// <summary>
    /// Approve a request step
    /// Approvers can approve steps they're assigned to, even without general approve permission
    /// </summary>
    [HttpPost("{requestId}/steps/{stepId}/approve")]
    [Authorize]
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
    /// Submit quiz answers for an approval step
    /// All authenticated users can submit quiz answers - handler verifies if user is the approver for this step
    /// </summary>
    [HttpPost("{requestId}/steps/{stepId}/quiz")]
    [Authorize]
    public async Task<ActionResult> SubmitQuiz(Guid requestId, Guid stepId, [FromBody] SubmitQuizDto dto)
    {
        var unauthorizedResult = GetUserIdOrUnauthorized(out var userGuid);
        if (unauthorizedResult != null)
        {
            return unauthorizedResult;
        }

        var command = new SubmitQuizAnswersCommand
        {
            RequestId = requestId,
            StepId = stepId,
            ApproverId = userGuid,
            Answers = dto.Answers.Select(a => new QuizAnswerSubmission
            {
                QuestionId = a.QuestionId,
                SelectedAnswer = a.SelectedAnswer
            }).ToList()
        };

        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Reject a request step
    /// Approvers can reject steps they're assigned to, even without general approve permission
    /// </summary>
    [HttpPost("{requestId}/steps/{stepId}/reject")]
    [Authorize]
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

