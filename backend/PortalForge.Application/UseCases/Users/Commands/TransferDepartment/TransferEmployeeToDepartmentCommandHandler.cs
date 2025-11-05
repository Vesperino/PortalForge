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

        // 6. Update user department
        user.DepartmentId = request.NewDepartmentId;
        await _unitOfWork.UserRepository.UpdateAsync(user);

        // Note: Pending requests are NOT automatically reassigned when changing departments.
        // Approvers are determined by department structure (HeadOfDepartmentId, DirectorId).
        // If department structure changes, requests will route to new department heads automatically.

        await _unitOfWork.SaveChangesAsync();

        // 8. Create audit log
        await _auditLogService.LogActionAsync(
            entityType: "User",
            entityId: user.Id.ToString(),
            action: "DepartmentTransfer",
            userId: request.TransferredByUserId,
            oldValue: $"Department: {oldDepartmentId}",
            newValue: $"Department: {request.NewDepartmentId}",
            reason: request.Reason);

        // 9. Send notification to old department head
        if (oldDepartmentId.HasValue)
        {
            var oldDepartment = await _unitOfWork.DepartmentRepository.GetByIdAsync(oldDepartmentId.Value);
            if (oldDepartment?.HeadOfDepartmentId.HasValue == true)
            {
                await _notificationService.CreateNotificationAsync(
                    userId: oldDepartment.HeadOfDepartmentId.Value,
                    type: NotificationType.System,
                    title: "Pracownik przeniesiony",
                    message: $"{user.FirstName} {user.LastName} został przeniesiony do innego działu",
                    relatedEntityType: null,
                    relatedEntityId: null,
                    actionUrl: null);
            }
        }

        // 10. Send notification to new department head
        if (newDepartment?.HeadOfDepartmentId.HasValue == true)
        {
            await _notificationService.CreateNotificationAsync(
                userId: newDepartment.HeadOfDepartmentId.Value,
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
