using MediatR;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.UseCases.Requests.Commands.ApproveRequestStep;

public class ApproveRequestStepCommandHandler
    : IRequestHandler<ApproveRequestStepCommand, ApproveRequestStepResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public ApproveRequestStepCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<ApproveRequestStepResult> Handle(
        ApproveRequestStepCommand command, 
        CancellationToken cancellationToken)
    {
        var request = await _unitOfWork.RequestRepository.GetByIdAsync(command.RequestId);
        if (request == null)
        {
            return new ApproveRequestStepResult
            {
                Success = false,
                Message = "Request not found"
            };
        }

        var step = request.ApprovalSteps.FirstOrDefault(s => s.Id == command.StepId);
        if (step == null)
        {
            return new ApproveRequestStepResult
            {
                Success = false,
                Message = "Approval step not found"
            };
        }

        if (step.ApproverId != command.ApproverId)
        {
            return new ApproveRequestStepResult
            {
                Success = false,
                Message = "Unauthorized: You are not the approver for this step"
            };
        }

        if (step.Status != ApprovalStepStatus.InReview)
        {
            return new ApproveRequestStepResult
            {
                Success = false,
                Message = "This step is not in review status"
            };
        }

        // Check if quiz is required and not passed
        if (step.RequiresQuiz && step.QuizPassed != true)
        {
            step.Status = ApprovalStepStatus.RequiresSurvey;
            request.Status = RequestStatus.AwaitingSurvey;
            await _unitOfWork.SaveChangesAsync();
            
            return new ApproveRequestStepResult
            {
                Success = false,
                Message = "Quiz must be completed before approval"
            };
        }

        // Approve the step
        step.Status = ApprovalStepStatus.Approved;
        step.FinishedAt = DateTime.UtcNow;
        step.Comment = command.Comment;

        // Check if there are more pending steps
        var nextStep = request.ApprovalSteps
            .Where(s => s.Status == ApprovalStepStatus.Pending)
            .OrderBy(s => s.StepOrder)
            .FirstOrDefault();

        if (nextStep != null)
        {
            // Move to next step
            nextStep.Status = ApprovalStepStatus.InReview;
            nextStep.StartedAt = DateTime.UtcNow;
            request.Status = RequestStatus.InReview;

            // Notify next approver
            await _notificationService.NotifyApproverAsync(nextStep.ApproverId, request);
        }
        else
        {
            // All steps approved
            request.Status = RequestStatus.Approved;
            request.CompletedAt = DateTime.UtcNow;

            // Notify submitter of completion
            await _notificationService.NotifySubmitterAsync(
                request,
                "Twój wniosek został zatwierdzony i zakończony pomyślnie.",
                NotificationType.RequestCompleted
            );
        }

        await _unitOfWork.RequestRepository.UpdateAsync(request);
        await _unitOfWork.SaveChangesAsync();

        return new ApproveRequestStepResult
        {
            Success = true,
            Message = request.Status == RequestStatus.Approved
                ? "Request fully approved"
                : "Step approved, moved to next approver"
        };
    }
}

