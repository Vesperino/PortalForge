using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;
using PortalForge.Application.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.UseCases.Users.Commands.TransferDepartment;

/// <summary>
/// Handler for TransferEmployeeToDepartmentCommand.
/// Transfers employee to new department and reassigns pending requests.
/// </summary>
public class TransferEmployeeToDepartmentCommandHandler : IRequestHandler<TransferEmployeeToDepartmentCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;
    private readonly IUnifiedValidatorService _validatorService;
    private readonly ILogger<TransferEmployeeToDepartmentCommandHandler> _logger;

    public TransferEmployeeToDepartmentCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditLogService auditLogService,
        INotificationService notificationService,
        IUnifiedValidatorService validatorService,
        ILogger<TransferEmployeeToDepartmentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _auditLogService = auditLogService;
        _notificationService = notificationService;
        _validatorService = validatorService;
        _logger = logger;
    }

    public async Task<Unit> Handle(
        TransferEmployeeToDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validate
        await _validatorService.ValidateAsync(request);

        // 2. Get user and validate existence
        var user = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId)
            ?? throw new NotFoundException($"User with ID {request.UserId} not found");

        // 3. Get new department and validate existence
        var newDepartment = await _unitOfWork.DepartmentRepository.GetByIdAsync(request.NewDepartmentId)
            ?? throw new NotFoundException($"Department with ID {request.NewDepartmentId} not found");

        // 4. Validate new supervisor if provided
        if (request.NewSupervisorId.HasValue)
        {
            var newSupervisor = await _unitOfWork.UserRepository.GetByIdAsync(request.NewSupervisorId.Value)
                ?? throw new NotFoundException($"New supervisor with ID {request.NewSupervisorId.Value} not found");
        }

        // 5. Store old values for audit
        var oldDepartmentId = user.DepartmentId;
        var oldSupervisorId = user.SupervisorId;

        // 6. Update user department and supervisor
        user.DepartmentId = request.NewDepartmentId;
        user.SupervisorId = request.NewSupervisorId;
        await _unitOfWork.UserRepository.UpdateAsync(user);

        // 7. Reassign pending requests to new supervisor
        if (request.NewSupervisorId.HasValue && oldSupervisorId.HasValue)
        {
            var pendingRequests = await _unitOfWork.RequestRepository
                .GetPendingRequestsByUserAsync(request.UserId);

            foreach (var req in pendingRequests)
            {
                var pendingSteps = req.ApprovalSteps
                    .Where(s => s.Status == ApprovalStepStatus.InReview ||
                               s.Status == ApprovalStepStatus.Pending);

                foreach (var step in pendingSteps)
                {
                    if (step.ApproverId == oldSupervisorId.Value)
                    {
                        step.ApproverId = request.NewSupervisorId.Value;
                        _logger.LogInformation(
                            "Reassigned approval step in request {RequestId} from {OldSupervisor} to {NewSupervisor}",
                            req.Id, oldSupervisorId.Value, request.NewSupervisorId.Value);
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }

        // 8. Create audit log
        await _auditLogService.LogActionAsync(
            entityType: "User",
            entityId: user.Id.ToString(),
            action: "DepartmentTransfer",
            userId: request.TransferredByUserId,
            oldValue: $"Department: {oldDepartmentId}, Supervisor: {oldSupervisorId}",
            newValue: $"Department: {request.NewDepartmentId}, Supervisor: {request.NewSupervisorId}",
            reason: request.Reason);

        // 9. Send notifications to old supervisor
        if (oldSupervisorId.HasValue)
        {
            await _notificationService.CreateNotificationAsync(
                userId: oldSupervisorId.Value,
                type: NotificationType.System,
                title: "Pracownik przeniesiony",
                message: $"{user.FirstName} {user.LastName} został przeniesiony do innego działu",
                relatedEntityType: null,
                relatedEntityId: null,
                actionUrl: null);
        }

        // 10. Send notifications to new supervisor
        if (request.NewSupervisorId.HasValue)
        {
            await _notificationService.CreateNotificationAsync(
                userId: request.NewSupervisorId.Value,
                type: NotificationType.System,
                title: "Nowy pracownik",
                message: $"{user.FirstName} {user.LastName} został przeniesiony do Twojego działu",
                relatedEntityType: "User",
                relatedEntityId: user.Id.ToString(),
                actionUrl: $"/dashboard/users/{user.Id}");
        }

        _logger.LogInformation(
            "User {UserId} transferred from department {OldDepartmentId} to {NewDepartmentId} by {TransferredByUserId}",
            user.Id, oldDepartmentId, request.NewDepartmentId, request.TransferredByUserId);

        return Unit.Value;
    }
}
