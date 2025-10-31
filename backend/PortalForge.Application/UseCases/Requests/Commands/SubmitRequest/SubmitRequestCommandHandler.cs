using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.UseCases.Requests.Commands.SubmitRequest;

public class SubmitRequestCommandHandler
    : IRequestHandler<SubmitRequestCommand, SubmitRequestResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IRequestRoutingService _routingService;
    private readonly ILogger<SubmitRequestCommandHandler> _logger;

    public SubmitRequestCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        IRequestRoutingService routingService,
        ILogger<SubmitRequestCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _routingService = routingService;
        _logger = logger;
    }

    public async Task<SubmitRequestResult> Handle(
        SubmitRequestCommand command, 
        CancellationToken cancellationToken)
    {
        // Get template with approval steps
        var template = await _unitOfWork.RequestTemplateRepository.GetByIdAsync(command.RequestTemplateId);
        if (template == null)
        {
            throw new Exception("Request template not found");
        }

        // Get submitter
        var submitter = await _unitOfWork.UserRepository.GetByIdAsync(command.SubmittedById);
        if (submitter == null)
        {
            throw new Exception("User not found");
        }

        // Generate request number
        var year = DateTime.UtcNow.Year;
        var allRequests = await _unitOfWork.RequestRepository.GetAllAsync();
        var requestCount = allRequests.Count() + 1;
        var requestNumber = $"REQ-{year}-{requestCount:D4}";

        // Create request
        var request = new Request
        {
            Id = Guid.NewGuid(),
            RequestNumber = requestNumber,
            RequestTemplateId = template.Id,
            SubmittedById = submitter.Id,
            SubmittedAt = DateTime.UtcNow,
            Priority = Enum.Parse<RequestPriority>(command.Priority),
            FormData = command.FormData,
            Status = template.RequiresApproval ? RequestStatus.InReview : RequestStatus.Approved
        };

        // Create approval steps based on template
        if (template.RequiresApproval && template.ApprovalStepTemplates.Any())
        {
            var orderedSteps = template.ApprovalStepTemplates.OrderBy(ast => ast.StepOrder).ToList();
            var lastCompletedStepOrder = 0;

            foreach (var stepTemplate in orderedSteps)
            {
                // Resolve approver using routing service
                var approver = await _routingService.ResolveApproverAsync(stepTemplate, submitter);

                if (approver == null)
                {
                    // No approver found - AUTO-APPROVE this step
                    var autoApprovedStep = new RequestApprovalStep
                    {
                        Id = Guid.NewGuid(),
                        RequestId = request.Id,
                        StepOrder = stepTemplate.StepOrder,
                        ApproverId = submitter.Id,
                        Status = ApprovalStepStatus.Approved,
                        RequiresQuiz = false,
                        Comment = "Auto-approved - submitter has no higher supervisor for this approval level",
                        StartedAt = DateTime.UtcNow,
                        FinishedAt = DateTime.UtcNow
                    };
                    request.ApprovalSteps.Add(autoApprovedStep);
                    lastCompletedStepOrder = stepTemplate.StepOrder;

                    _logger.LogInformation(
                        "Auto-approved step {StepOrder} for request {RequestId} - user {UserId} has no higher supervisor",
                        stepTemplate.StepOrder, request.Id, submitter.Id);
                }
                else
                {
                    // Normal approval step
                    var isFirstPendingStep = stepTemplate.StepOrder == lastCompletedStepOrder + 1;

                    var approvalStep = new RequestApprovalStep
                    {
                        Id = Guid.NewGuid(),
                        RequestId = request.Id,
                        StepOrder = stepTemplate.StepOrder,
                        ApproverId = approver.Id,
                        Status = isFirstPendingStep
                            ? ApprovalStepStatus.InReview
                            : ApprovalStepStatus.Pending,
                        RequiresQuiz = stepTemplate.RequiresQuiz,
                        StartedAt = isFirstPendingStep ? DateTime.UtcNow : null
                    };
                    request.ApprovalSteps.Add(approvalStep);

                    _logger.LogDebug(
                        "Created approval step {StepOrder} for request {RequestId}, approver {ApproverId}, status {Status}",
                        stepTemplate.StepOrder, request.Id, approver.Id, approvalStep.Status);
                }
            }
        }

        await _unitOfWork.RequestRepository.CreateAsync(request);
        await _unitOfWork.SaveChangesAsync();

        // Send notifications to first step approvers
        var firstStepApprovers = request.ApprovalSteps
            .Where(s => s.StepOrder == 1)
            .Select(s => s.ApproverId)
            .Distinct();

        foreach (var approverId in firstStepApprovers)
        {
            await _notificationService.NotifyApproverAsync(approverId, request);
        }

        return new SubmitRequestResult
        {
            Id = request.Id,
            RequestNumber = request.RequestNumber,
            Message = "Request submitted successfully"
        };
    }
}

