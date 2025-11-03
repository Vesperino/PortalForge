using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Exceptions;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.UseCases.Users.Commands.UpdateVacationAllowance;

/// <summary>
/// Handler for UpdateUserVacationAllowanceCommand.
/// Updates user's annual vacation allowance with audit logging and notifications.
/// </summary>
public class UpdateUserVacationAllowanceCommandHandler : IRequestHandler<UpdateUserVacationAllowanceCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;
    private readonly IUnifiedValidatorService _validatorService;
    private readonly ILogger<UpdateUserVacationAllowanceCommandHandler> _logger;

    public UpdateUserVacationAllowanceCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditLogService auditLogService,
        INotificationService notificationService,
        IUnifiedValidatorService validatorService,
        ILogger<UpdateUserVacationAllowanceCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _auditLogService = auditLogService;
        _notificationService = notificationService;
        _validatorService = validatorService;
        _logger = logger;
    }

    public async Task<Unit> Handle(
        UpdateUserVacationAllowanceCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validate
        await _validatorService.ValidateAsync(request);

        // 2. Get user
        var user = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId)
            ?? throw new NotFoundException($"User with ID {request.UserId} not found");

        // 3. Save old value for audit
        var oldValue = user.AnnualVacationDays;

        // 4. Update vacation allowance
        user.AnnualVacationDays = request.NewAnnualDays;
        await _unitOfWork.UserRepository.UpdateAsync(user);

        // 5. Create audit log
        await _auditLogService.LogActionAsync(
            entityType: "User",
            entityId: user.Id.ToString(),
            action: "VacationAllowanceUpdated",
            userId: request.RequestedByUserId,
            oldValue: oldValue.ToString(),
            newValue: request.NewAnnualDays.ToString(),
            reason: request.Reason,
            ipAddress: request.IpAddress);

        // 6. Notify user
        await _notificationService.CreateNotificationAsync(
            userId: user.Id,
            type: NotificationType.VacationAllowanceUpdated,
            title: "Zmiana limitu urlopów",
            message: $"Twój limit urlopów został zmieniony z {oldValue} na {request.NewAnnualDays} dni. Powód: {request.Reason}",
            relatedEntityType: "User",
            relatedEntityId: user.Id.ToString(),
            actionUrl: "/dashboard/account");

        _logger.LogInformation(
            "Vacation allowance updated for user {UserId} from {OldValue} to {NewValue} by {RequestedByUserId}",
            user.Id, oldValue, request.NewAnnualDays, request.RequestedByUserId);

        return Unit.Value;
    }
}
