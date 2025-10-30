using MediatR;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.UseCases.Requests.Commands.RejectRequestStep;

public class RejectRequestStepCommandHandler 
    : IRequestHandler<RejectRequestStepCommand, RejectRequestStepResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public RejectRequestStepCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<RejectRequestStepResult> Handle(
        RejectRequestStepCommand command, 
        CancellationToken cancellationToken)
    {
        var request = await _unitOfWork.RequestRepository.GetByIdAsync(command.RequestId);
        if (request == null)
        {
            return new RejectRequestStepResult
            {
                Success = false,
                Message = "Request not found"
            };
        }

        var step = request.ApprovalSteps.FirstOrDefault(s => s.Id == command.StepId);
        if (step == null)
        {
            return new RejectRequestStepResult
            {
                Success = false,
                Message = "Approval step not found"
            };
        }

        if (step.ApproverId != command.ApproverId)
        {
            return new RejectRequestStepResult
            {
                Success = false,
                Message = "Unauthorized: You are not the approver for this step"
            };
        }

        if (step.Status != ApprovalStepStatus.InReview)
        {
            return new RejectRequestStepResult
            {
                Success = false,
                Message = "This step is not in review status"
            };
        }

        // Reject the step
        step.Status = ApprovalStepStatus.Rejected;
        step.FinishedAt = DateTime.UtcNow;
        step.Comment = command.Reason;

        // Reject the entire request
        request.Status = RequestStatus.Rejected;
        request.CompletedAt = DateTime.UtcNow;

        await _unitOfWork.RequestRepository.UpdateAsync(request);
        await _unitOfWork.SaveChangesAsync();

        // Notify submitter of rejection
        await _notificationService.NotifySubmitterAsync(
            request,
            $"Twój wniosek został odrzucony. Powód: {command.Reason}",
            NotificationType.RequestRejected
        );

        return new RejectRequestStepResult
        {
            Success = true,
            Message = "Request rejected successfully"
        };
    }
}

