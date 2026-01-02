using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PortalForge.Application.Common.Settings;
using PortalForge.Application.Exceptions;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Users.Queries.GetUserVacationSummary;

/// <summary>
/// Handler for GetUserVacationSummaryQuery.
/// Retrieves user's vacation allowance, used days, and remaining days.
/// </summary>
public class GetUserVacationSummaryQueryHandler : IRequestHandler<GetUserVacationSummaryQuery, VacationSummaryDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly PortalForge.Application.Interfaces.IVacationCalculationService _vacationCalculationService;
    private readonly VacationSettings _vacationSettings;
    private readonly ILogger<GetUserVacationSummaryQueryHandler> _logger;

    public GetUserVacationSummaryQueryHandler(
        IUnitOfWork unitOfWork,
        PortalForge.Application.Interfaces.IVacationCalculationService vacationCalculationService,
        IOptions<VacationSettings> vacationSettings,
        ILogger<GetUserVacationSummaryQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _vacationCalculationService = vacationCalculationService;
        _vacationSettings = vacationSettings.Value;
        _logger = logger;
    }

    public async Task<VacationSummaryDto> Handle(
        GetUserVacationSummaryQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Retrieving vacation summary for user {UserId}",
            request.UserId);

        var user = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException($"User with ID {request.UserId} not found");

        var annualDays = user.AnnualVacationDays ?? _vacationSettings.DefaultAnnualDays;
        // Calculate used business days from actual vacation schedules
        var calcUsed = await _vacationCalculationService.CalculateVacationDaysUsedAsync(user.Id, DateTime.UtcNow.Year);
        // Some flows account for approved (future) vacations immediately in user's counters.
        // To align UI, prefer the larger of the two.
        var usedDays = Math.Max(calcUsed, user.VacationDaysUsed ?? 0);
        var onDemandUsed = user.OnDemandVacationDaysUsed ?? 0;
        var circumstantialUsed = user.CircumstantialLeaveDaysUsed ?? 0;
        var carriedOver = user.CarriedOverVacationDays ?? 0;

        return new VacationSummaryDto
        {
            AnnualVacationDays = annualDays,
            VacationDaysUsed = usedDays,
            VacationDaysRemaining = Math.Max(0, annualDays - usedDays),
            OnDemandVacationDaysUsed = onDemandUsed,
            OnDemandVacationDaysRemaining = _vacationSettings.MaxOnDemandDays - onDemandUsed,
            CircumstantialLeaveDaysUsed = circumstantialUsed,
            CarriedOverVacationDays = carriedOver,
            CarriedOverExpiryDate = user.CarriedOverExpiryDate,
            TotalAvailableVacationDays = annualDays + carriedOver - usedDays
        };
    }
}
