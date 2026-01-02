using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.UseCases.Requests.Commands.ApproveRequestStep;

public class ApproveRequestStepCommandHandler
    : IRequestHandler<ApproveRequestStepCommand, ApproveRequestStepResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IVacationCreationService _vacationCreationService;
    private readonly IVacationSubstituteService _vacationSubstituteService;
    private readonly IVacationDaysDeductionService _vacationDaysDeductionService;
    private readonly ILogger<ApproveRequestStepCommandHandler> _logger;

    public ApproveRequestStepCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        IVacationCreationService vacationCreationService,
        IVacationSubstituteService vacationSubstituteService,
        IVacationDaysDeductionService vacationDaysDeductionService,
        ILogger<ApproveRequestStepCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _vacationCreationService = vacationCreationService;
        _vacationSubstituteService = vacationSubstituteService;
        _vacationDaysDeductionService = vacationDaysDeductionService;
        _logger = logger;
    }

    public async Task<ApproveRequestStepResult> Handle(
        ApproveRequestStepCommand command,
        CancellationToken cancellationToken)
    {
        var request = await _unitOfWork.RequestRepository.GetByIdAsync(command.RequestId, cancellationToken);
        if (request == null)
        {
            return ApproveRequestStepResult.Failure("Request not found");
        }

        var step = request.ApprovalSteps.FirstOrDefault(s => s.Id == command.StepId);
        var validationResult = ValidateStep(step, command.ApproverId);
        if (validationResult != null)
        {
            return validationResult;
        }

        ApproveStep(step!, command.Comment);
        await CreateApprovalCommentIfNeeded(request, step!, command);

        var nextStep = GetNextPendingStep(request);
        if (nextStep != null)
        {
            await ActivateNextStepAsync(request, nextStep);
        }
        else
        {
            await CompleteRequestAsync(request);
        }

        await _unitOfWork.SaveChangesAsync();

        return ApproveRequestStepResult.Success(
            request.Status == RequestStatus.Approved
                ? "Request fully approved"
                : "Step approved, moved to next approver");
    }

    private static ApproveRequestStepResult? ValidateStep(RequestApprovalStep? step, Guid approverId)
    {
        if (step == null)
        {
            return ApproveRequestStepResult.Failure("Approval step not found");
        }

        if (step.ApproverId != approverId)
        {
            return ApproveRequestStepResult.Failure("Unauthorized: You are not the approver for this step");
        }

        if (step.Status != ApprovalStepStatus.InReview)
        {
            return ApproveRequestStepResult.Failure("This step is not in review status");
        }

        return null;
    }

    private static void ApproveStep(RequestApprovalStep step, string? comment)
    {
        step.Status = ApprovalStepStatus.Approved;
        step.FinishedAt = DateTime.UtcNow;
        step.Comment = BuildStepComment(step, comment);
    }

    private static string BuildStepComment(RequestApprovalStep step, string? comment)
    {
        var baseComment = comment ?? string.Empty;

        if (step.RequiresQuiz && step.QuizScore.HasValue && step.QuizPassed == false)
        {
            var overrideNote = $"[OVERRIDE] Approver approved despite quiz failure (scored {step.QuizScore}%)";
            return string.IsNullOrWhiteSpace(baseComment) ? overrideNote : $"{baseComment}\n\n{overrideNote}";
        }

        return baseComment;
    }

    private async Task CreateApprovalCommentIfNeeded(Request request, RequestApprovalStep step, ApproveRequestStepCommand command)
    {
        if (string.IsNullOrWhiteSpace(step.Comment))
        {
            return;
        }

        var requestComment = new RequestComment
        {
            Id = Guid.NewGuid(),
            RequestId = request.Id,
            UserId = command.ApproverId,
            Comment = $"✅ Zatwierdzono etap: {step.Comment}",
            Attachments = null,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.RequestCommentRepository.CreateAsync(requestComment);
    }

    private static RequestApprovalStep? GetNextPendingStep(Request request)
    {
        return request.ApprovalSteps
            .Where(s => s.Status == ApprovalStepStatus.Pending)
            .OrderBy(s => s.StepOrder)
            .FirstOrDefault();
    }

    private async Task ActivateNextStepAsync(Request request, RequestApprovalStep nextStep)
    {
        var substitute = await _vacationSubstituteService.GetActiveSubstituteAsync(nextStep.ApproverId);
        if (substitute != null)
        {
            nextStep.ApproverId = substitute.Id;
            nextStep.Comment = "Routed to substitute for approver on vacation";
        }

        nextStep.Status = ApprovalStepStatus.InReview;
        nextStep.StartedAt = DateTime.UtcNow;
        request.Status = RequestStatus.InReview;

        await _notificationService.NotifyApproverAsync(nextStep.ApproverId, request);
    }

    private async Task CompleteRequestAsync(Request request)
    {
        request.Status = RequestStatus.Approved;
        request.CompletedAt = DateTime.UtcNow;

        await ProcessVacationRequestAsync(request);
        await NotifySubmitterOfCompletionAsync(request);
    }

    private async Task ProcessVacationRequestAsync(Request request)
    {
        if (request.RequestTemplate?.IsVacationRequest != true)
        {
            return;
        }

        try
        {
            await _vacationDaysDeductionService.DeductVacationDaysAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deduct vacation days for request {RequestId}", request.Id);
        }

        try
        {
            await _vacationCreationService.CreateFromApprovedRequestAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create calendar entry for request {RequestId}", request.Id);
        }
    }

    private async Task NotifySubmitterOfCompletionAsync(Request request)
    {
        await _notificationService.NotifySubmitterAsync(
            request,
            "Wniosek został zatwierdzony i zakończony pomyślnie.",
            NotificationType.RequestCompleted);
    }
}
