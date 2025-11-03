using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;
using PortalForge.Application.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.UseCases.Vacations.Commands.CancelVacation;

/// <summary>
/// Handler for CancelVacationCommand.
/// Cancels active vacation with authorization checks, audit logging, and notifications.
/// </summary>
public class CancelVacationCommandHandler : IRequestHandler<CancelVacationCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IAuditLogService _auditLogService;
    private readonly IUnifiedValidatorService _validatorService;
    private readonly ILogger<CancelVacationCommandHandler> _logger;

    public CancelVacationCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        IAuditLogService auditLogService,
        IUnifiedValidatorService validatorService,
        ILogger<CancelVacationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _auditLogService = auditLogService;
        _validatorService = validatorService;
        _logger = logger;
    }

    public async Task<Unit> Handle(CancelVacationCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate
        await _validatorService.ValidateAsync(request);

        // 2. Get vacation schedule with related entities
        var vacation = await _unitOfWork.VacationScheduleRepository.GetByIdAsync(request.VacationScheduleId)
            ?? throw new NotFoundException($"Vacation schedule with ID {request.VacationScheduleId} not found");

        var cancelledBy = await _unitOfWork.UserRepository.GetByIdAsync(request.CancelledByUserId)
            ?? throw new NotFoundException($"User with ID {request.CancelledByUserId} not found");

        var employee = await _unitOfWork.UserRepository.GetByIdAsync(vacation.UserId)
            ?? throw new NotFoundException($"Employee with ID {vacation.UserId} not found");

        // 3. Check authorization
        var isAdmin = cancelledBy.Role == UserRole.Admin;
        var isApprover = vacation.SourceRequest?.ApprovalSteps
            .Any(s => s.ApproverId == request.CancelledByUserId && s.Status == ApprovalStepStatus.Approved) ?? false;

        var daysSinceStart = (DateTime.UtcNow - vacation.StartDate).Days;

        if (!isAdmin)
        {
            if (!isApprover)
            {
                throw new ForbiddenException("Nie masz uprawnień do anulowania tego urlopu");
            }

            if (daysSinceStart > 1)
            {
                throw new ValidationException(
                    "Przełożony może anulować urlop tylko do 1 dnia po jego rozpoczęciu. Skontaktuj się z administratorem.");
            }
        }

        // 4. Cancel vacation
        var oldStatus = vacation.Status;
        vacation.Status = VacationStatus.Cancelled;
        await _unitOfWork.VacationScheduleRepository.UpdateAsync(vacation);

        // 5. Return days to user's vacation pool
        employee.VacationDaysUsed -= vacation.DaysCount;
        await _unitOfWork.UserRepository.UpdateAsync(employee);

        // 6. Audit log
        await _auditLogService.LogActionAsync(
            entityType: "VacationSchedule",
            entityId: vacation.Id.ToString(),
            action: "VacationCancelled",
            userId: request.CancelledByUserId,
            oldValue: oldStatus.ToString(),
            newValue: "Cancelled",
            reason: request.Reason);

        // 7. Notify employee
        await _notificationService.CreateNotificationAsync(
            userId: vacation.UserId,
            type: NotificationType.VacationCancelled,
            title: "Urlop został anulowany",
            message: $"Twój urlop od {vacation.StartDate:dd.MM.yyyy} do {vacation.EndDate:dd.MM.yyyy} został anulowany. " +
                     $"Zwrócono {vacation.DaysCount} dni do Twojej puli. Powód: {request.Reason}",
            relatedEntityType: "VacationSchedule",
            relatedEntityId: vacation.Id.ToString(),
            actionUrl: "/dashboard/account");

        // 8. Notify substitute if exists
        if (vacation.SubstituteUserId != Guid.Empty)
        {
            await _notificationService.CreateNotificationAsync(
                userId: vacation.SubstituteUserId,
                type: NotificationType.System,
                title: "Urlop został anulowany",
                message: $"Urlop {employee.FirstName} {employee.LastName}, dla którego byłeś/aś zastępcą, został anulowany.",
                relatedEntityType: null,
                relatedEntityId: null,
                actionUrl: null);
        }

        _logger.LogInformation(
            "Vacation {VacationId} cancelled by user {UserId}. Returned {DaysCount} days to employee {EmployeeId}",
            vacation.Id, request.CancelledByUserId, vacation.DaysCount, employee.Id);

        return Unit.Value;
    }
}
