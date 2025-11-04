using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;
using PortalForge.Application.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.UseCases.Requests.Commands.EditRequest;

/// <summary>
/// Handler for EditRequestCommand.
/// Edits existing request with full history tracking and notifications.
/// </summary>
public class EditRequestCommandHandler : IRequestHandler<EditRequestCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IUnifiedValidatorService _validatorService;
    private readonly ILogger<EditRequestCommandHandler> _logger;

    public EditRequestCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        IUnifiedValidatorService validatorService,
        ILogger<EditRequestCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _validatorService = validatorService;
        _logger = logger;
    }

    public async Task<Unit> Handle(EditRequestCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate
        await _validatorService.ValidateAsync(request);

        // 2. Get request with approval steps
        var existingRequest = await _unitOfWork.RequestRepository.GetByIdAsync(request.RequestId)
            ?? throw new NotFoundException($"Request with ID {request.RequestId} not found");

        // 3. Authorization check - only submitter can edit
        if (existingRequest.SubmittedById != request.EditedByUserId)
        {
            throw new ForbiddenException("Możesz edytować tylko własne wnioski");
        }

        // 4. Status check - only Draft or InReview can be edited
        if (existingRequest.Status != RequestStatus.Draft &&
            existingRequest.Status != RequestStatus.InReview)
        {
            throw new ValidationException(
                "Możesz edytować tylko wnioski ze statusem Draft lub InReview. " +
                "Wnioski zatwierdzone, odrzucone lub zakończone nie mogą być edytowane.");
        }

        // 5. Create edit history record
        var editHistory = new RequestEditHistory
        {
            Id = Guid.NewGuid(),
            RequestId = request.RequestId,
            EditedByUserId = request.EditedByUserId,
            EditedAt = DateTime.UtcNow,
            OldFormData = existingRequest.FormData,
            NewFormData = request.NewFormData,
            ChangeReason = request.ChangeReason
        };

        await _unitOfWork.RequestEditHistoryRepository.CreateAsync(editHistory);

        // 6. Update request
        existingRequest.FormData = request.NewFormData;
        await _unitOfWork.RequestRepository.UpdateAsync(existingRequest);

        // 7. Notify approvers who already reviewed this request
        var reviewedApprovers = existingRequest.ApprovalSteps
            .Where(s => s.Status == ApprovalStepStatus.Approved ||
                       s.Status == ApprovalStepStatus.Rejected)
            .Select(s => s.ApproverId)
            .Distinct();

        foreach (var approverId in reviewedApprovers)
        {
            await _notificationService.CreateNotificationAsync(
                userId: approverId,
                type: NotificationType.RequestEdited,
                title: "Wniosek został edytowany",
                message: $"Wniosek {existingRequest.RequestNumber} został edytowany przez składającego. " +
                         $"Powód: {request.ChangeReason ?? "Brak"}",
                relatedEntityType: "Request",
                relatedEntityId: request.RequestId.ToString(),
                actionUrl: $"/dashboard/requests/{request.RequestId}");
        }

        _logger.LogInformation(
            "Request {RequestId} edited by user {UserId}. Reason: {Reason}",
            request.RequestId, request.EditedByUserId, request.ChangeReason);

        return Unit.Value;
    }
}
