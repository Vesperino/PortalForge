using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Exceptions;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.UseCases.Users.Commands.UpdateFullVacationData;

/// <summary>
/// Handler for UpdateFullVacationDataCommand.
/// Updates ALL vacation-related fields for a user with full audit logging.
/// </summary>
public class UpdateFullVacationDataCommandHandler : IRequestHandler<UpdateFullVacationDataCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;
    private readonly IUnifiedValidatorService _validatorService;
    private readonly ILogger<UpdateFullVacationDataCommandHandler> _logger;

    public UpdateFullVacationDataCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditLogService auditLogService,
        INotificationService notificationService,
        IUnifiedValidatorService validatorService,
        ILogger<UpdateFullVacationDataCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _auditLogService = auditLogService;
        _notificationService = notificationService;
        _validatorService = validatorService;
        _logger = logger;
    }

    public async Task<Unit> Handle(
        UpdateFullVacationDataCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validate
        await _validatorService.ValidateAsync(request);

        // 2. Get user
        var user = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId)
            ?? throw new NotFoundException($"User with ID {request.UserId} not found");

        // 3. Save old values for audit
        var oldData = new
        {
            AnnualVacationDays = user.AnnualVacationDays ?? 0,
            VacationDaysUsed = user.VacationDaysUsed ?? 0,
            OnDemandVacationDaysUsed = user.OnDemandVacationDaysUsed ?? 0,
            CircumstantialLeaveDaysUsed = user.CircumstantialLeaveDaysUsed ?? 0,
            CarriedOverVacationDays = user.CarriedOverVacationDays ?? 0,
            CarriedOverExpiryDate = user.CarriedOverExpiryDate
        };

        // 4. Update ALL vacation fields
        user.AnnualVacationDays = request.AnnualVacationDays;
        user.VacationDaysUsed = request.VacationDaysUsed;
        user.OnDemandVacationDaysUsed = request.OnDemandVacationDaysUsed;
        user.CircumstantialLeaveDaysUsed = request.CircumstantialLeaveDaysUsed;
        user.CarriedOverVacationDays = request.CarriedOverVacationDays;
        user.CarriedOverExpiryDate = request.CarriedOverExpiryDate;

        await _unitOfWork.UserRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // 5. Create detailed audit log
        var newData = new
        {
            AnnualVacationDays = request.AnnualVacationDays,
            VacationDaysUsed = request.VacationDaysUsed,
            OnDemandVacationDaysUsed = request.OnDemandVacationDaysUsed,
            CircumstantialLeaveDaysUsed = request.CircumstantialLeaveDaysUsed,
            CarriedOverVacationDays = request.CarriedOverVacationDays,
            CarriedOverExpiryDate = request.CarriedOverExpiryDate
        };

        await _auditLogService.LogActionAsync(
            entityType: "User",
            entityId: user.Id.ToString(),
            action: "FullVacationDataUpdated",
            userId: request.RequestedByUserId,
            oldValue: System.Text.Json.JsonSerializer.Serialize(oldData),
            newValue: System.Text.Json.JsonSerializer.Serialize(newData),
            reason: request.Reason,
            ipAddress: request.IpAddress);

        // 6. Notify user
        var availableDays = request.AnnualVacationDays + request.CarriedOverVacationDays - request.VacationDaysUsed;
        await _notificationService.CreateNotificationAsync(
            userId: user.Id,
            type: NotificationType.VacationAllowanceUpdated,
            title: "Aktualizacja danych urlopowych",
            message: $"Twoje dane urlopowe zostały zaktualizowane. Dostępne dni: {availableDays}. Powód: {request.Reason}",
            relatedEntityType: "User",
            relatedEntityId: user.Id.ToString(),
            actionUrl: "/dashboard/account");

        _logger.LogInformation(
            "Full vacation data updated for user {UserId} by {RequestedByUserId}. Available days: {AvailableDays}",
            user.Id, request.RequestedByUserId, availableDays);

        return Unit.Value;
    }
}
