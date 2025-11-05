using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.UseCases.Requests.Commands.BulkApproveRequests;

public class BulkApproveRequestsCommandHandler : IRequestHandler<BulkApproveRequestsCommand, BulkApproveRequestsResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IEnhancedRequestRoutingService _routingService;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<BulkApproveRequestsCommandHandler> _logger;

    public BulkApproveRequestsCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        IEnhancedRequestRoutingService routingService,
        IAuditLogService auditLogService,
        ILogger<BulkApproveRequestsCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _routingService = routingService;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<BulkApproveRequestsResult> Handle(BulkApproveRequestsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing bulk approval for {StepCount} steps by user {ApproverId}",
            request.RequestStepIds.Count,
            request.ApproverId);

        var result = new BulkApproveRequestsResult();
        var approver = await _unitOfWork.UserRepository.GetByIdAsync(request.ApproverId);

        if (approver == null)
        {
            result.Success = false;
            result.Message = "Approver not found";
            return result;
        }

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            foreach (var stepId in request.RequestStepIds)
            {
                try
                {
                    var stepResult = await ProcessSingleApprovalAsync(stepId, approver, request.Comment, request.SkipValidation);
                    
                    if (stepResult.Success)
                    {
                        result.SuccessfulApprovals++;
                    }
                    else
                    {
                        result.FailedApprovals++;
                        result.Errors.Add(new BulkApprovalError
                        {
                            RequestStepId = stepId,
                            RequestTitle = stepResult.RequestTitle,
                            ErrorMessage = stepResult.ErrorMessage
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing approval for step {StepId}", stepId);
                    result.FailedApprovals++;
                    result.Errors.Add(new BulkApprovalError
                    {
                        RequestStepId = stepId,
                        RequestTitle = "Unknown",
                        ErrorMessage = ex.Message
                    });
                }
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            result.Success = result.SuccessfulApprovals > 0;
            result.Message = result.Success 
                ? $"Bulk approval completed: {result.SuccessfulApprovals} successful, {result.FailedApprovals} failed"
                : "Bulk approval failed: No approvals were processed successfully";

            // Log bulk approval action
            await _auditLogService.LogActionAsync(
                "Request",
                "BulkApproval",
                $"Bulk approved {result.SuccessfulApprovals} requests",
                request.ApproverId,
                null,
                $"SuccessfulApprovals: {result.SuccessfulApprovals}, FailedApprovals: {result.FailedApprovals}",
                "Bulk approval operation");

            _logger.LogInformation(
                "Bulk approval completed: {SuccessfulCount} successful, {FailedCount} failed",
                result.SuccessfulApprovals,
                result.FailedApprovals);

            return result;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error during bulk approval transaction");
            
            result.Success = false;
            result.Message = "Bulk approval failed due to transaction error";
            return result;
        }
    }

    private async Task<SingleApprovalResult> ProcessSingleApprovalAsync(
        Guid stepId, 
        User approver, 
        string? comment, 
        bool skipValidation)
    {
        var step = await _unitOfWork.RequestApprovalStepRepository.GetByIdAsync(stepId);
        if (step == null)
        {
            return new SingleApprovalResult
            {
                Success = false,
                ErrorMessage = "Approval step not found",
                RequestTitle = "Unknown"
            };
        }

        var request = step.Request;
        if (request == null)
        {
            return new SingleApprovalResult
            {
                Success = false,
                ErrorMessage = "Associated request not found",
                RequestTitle = "Unknown"
            };
        }

        // Validation checks (unless skipped)
        if (!skipValidation)
        {
            // Check if user is authorized to approve this step
            var effectiveApproverId = step.AssignedToUserId ?? step.ApproverId;
            if (effectiveApproverId != approver.Id)
            {
                // Check if user has delegation authority
                var delegatedApprovers = await _routingService.GetDelegatedApproversAsync(effectiveApproverId);
                if (!delegatedApprovers.Any(d => d.Id == approver.Id))
                {
                    return new SingleApprovalResult
                    {
                        Success = false,
                        ErrorMessage = "User not authorized to approve this step",
                        RequestTitle = request.RequestTemplate?.Name ?? "Unknown Request"
                    };
                }
            }

            // Check if step is in correct status
            if (step.Status != ApprovalStepStatus.InReview)
            {
                return new SingleApprovalResult
                {
                    Success = false,
                    ErrorMessage = $"Step is not in review status (current: {step.Status})",
                    RequestTitle = request.RequestTemplate?.Name ?? "Unknown Request"
                };
            }

            // Check if quiz is required and completed
            if (step.RequiresQuiz && (!step.QuizPassed.HasValue || !step.QuizPassed.Value))
            {
                return new SingleApprovalResult
                {
                    Success = false,
                    ErrorMessage = "Quiz must be completed before approval",
                    RequestTitle = request.RequestTemplate?.Name ?? "Unknown Request"
                };
            }
        }

        // Process the approval
        step.Status = ApprovalStepStatus.Approved;
        step.FinishedAt = DateTime.UtcNow;
        step.Comment = string.IsNullOrEmpty(comment) ? "Bulk approved" : $"Bulk approved: {comment}";

        await _unitOfWork.RequestApprovalStepRepository.UpdateAsync(step);

        // Check if this completes the request or moves to next step
        await ProcessRequestAfterApprovalAsync(request);

        // Send notification
        try
        {
            await _notificationService.NotifySubmitterAsync(
                request,
                $"Your request '{request.RequestTemplate?.Name ?? "Unknown Request"}' has been approved (bulk approval)",
                Domain.Enums.NotificationType.RequestApproved);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send notification for request {RequestId}", request.Id);
            // Don't fail the approval due to notification issues
        }

        return new SingleApprovalResult
        {
            Success = true,
            RequestTitle = request.RequestTemplate?.Name ?? "Unknown Request"
        };
    }

    private async Task ProcessRequestAfterApprovalAsync(Request request)
    {
        var allSteps = request.ApprovalSteps.OrderBy(s => s.StepOrder).ToList();
        var currentStepIndex = allSteps.FindIndex(s => s.Status == ApprovalStepStatus.Approved && s.FinishedAt.HasValue);

        if (currentStepIndex == -1)
        {
            return; // No approved step found
        }

        // Check if there are more steps
        var nextStepIndex = currentStepIndex + 1;
        if (nextStepIndex < allSteps.Count)
        {
            // Move to next step
            var nextStep = allSteps[nextStepIndex];
            nextStep.Status = ApprovalStepStatus.InReview;
            nextStep.StartedAt = DateTime.UtcNow;
            
            await _unitOfWork.RequestApprovalStepRepository.UpdateAsync(nextStep);
            
            request.Status = RequestStatus.InReview;
        }
        else
        {
            // All steps completed - approve the request
            request.Status = RequestStatus.Approved;
            request.CompletedAt = DateTime.UtcNow;
        }

        await _unitOfWork.RequestRepository.UpdateAsync(request);
    }

    private class SingleApprovalResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string RequestTitle { get; set; } = string.Empty;
    }
}