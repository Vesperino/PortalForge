using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;
using PortalForge.Domain.Entities;
using System.Text.Json;

namespace PortalForge.Application.UseCases.Admin.Commands.AdjustVacationDays;

public class AdjustVacationDaysCommandHandler
    : IRequestHandler<AdjustVacationDaysCommand, AdjustVacationDaysResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUnifiedValidatorService _validatorService;
    private readonly ILogger<AdjustVacationDaysCommandHandler> _logger;

    public AdjustVacationDaysCommandHandler(
        IUnitOfWork unitOfWork,
        IUnifiedValidatorService validatorService,
        ILogger<AdjustVacationDaysCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _validatorService = validatorService;
        _logger = logger;
    }

    public async Task<AdjustVacationDaysResult> Handle(
        AdjustVacationDaysCommand request,
        CancellationToken cancellationToken)
    {
        // Validate
        await _validatorService.ValidateAsync(request);

        _logger.LogInformation(
            "Adjusting vacation days for user {UserId} by {Amount} days. Reason: {Reason}",
            request.UserId, request.AdjustmentAmount, request.Reason);

        // Get user
        var user = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            throw new NotFoundException($"User with ID {request.UserId} not found");
        }

        // Store old value for audit
        var oldValue = user.AnnualVacationDays ?? 26;

        // Calculate new value
        var newValue = oldValue + request.AdjustmentAmount;

        // Ensure new value is not negative
        if (newValue < 0)
        {
            throw new ValidationException(
                "Invalid adjustment",
                new List<string> { "Adjusted vacation days cannot be negative" });
        }

        // Apply adjustment
        user.AnnualVacationDays = newValue;

        // Save changes
        await _unitOfWork.UserRepository.UpdateAsync(user);

        // Create audit log
        var auditData = new
        {
            UserId = user.Id,
            UserFullName = user.FullName,
            OldValue = oldValue,
            NewValue = newValue,
            AdjustmentAmount = request.AdjustmentAmount,
            Reason = request.Reason,
            AdjustedBy = request.AdjustedBy
        };

        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = request.AdjustedBy,
            Action = "AdjustVacationDays",
            EntityType = "User",
            EntityId = user.Id.ToString(),
            OldValue = oldValue.ToString(),
            NewValue = newValue.ToString(),
            Reason = request.Reason,
            Timestamp = DateTime.UtcNow
        };

        await _unitOfWork.AuditLogRepository.CreateAsync(auditLog);

        _logger.LogInformation(
            "Vacation days adjusted for user {UserId}. Old: {OldValue}, New: {NewValue}",
            user.Id, oldValue, newValue);

        return new AdjustVacationDaysResult
        {
            Success = true,
            Message = $"Vacation days adjusted successfully from {oldValue} to {newValue}",
            OldValue = oldValue,
            NewValue = newValue
        };
    }
}
