using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Interfaces;
using PortalForge.Domain.Enums;

namespace PortalForge.Infrastructure.Services;

/// <summary>
/// Service for calculating vacation allowances and validating vacation availability.
/// Implements Polish labor law requirements (Kodeks Pracy 2025).
/// </summary>
public class VacationCalculationService : IVacationCalculationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VacationCalculationService> _logger;

    public VacationCalculationService(
        IUnitOfWork unitOfWork,
        ILogger<VacationCalculationService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Calculates proportional vacation days based on employment start date.
    /// According to Polish law (Art. 155¹ KP): employee gets proportional vacation
    /// based on months worked in the first year.
    /// Formula: (annualDays / 12) * months remaining in year
    /// Rounding: Always round UP to full days (per Polish law)
    /// </summary>
    public int CalculateProportionalVacationDays(DateTime employmentStartDate, int annualDays = 26)
    {
        var currentYear = DateTime.UtcNow.Year;
        var startYear = employmentStartDate.Year;

        // If employed in previous years, entitled to full annual allowance
        if (startYear < currentYear)
        {
            _logger.LogDebug(
                "Employee started in previous year ({StartYear}), entitled to full {AnnualDays} days",
                startYear, annualDays);
            return annualDays;
        }

        // Calculate months remaining in the year (inclusive)
        var monthsRemaining = 12 - employmentStartDate.Month + 1;

        // Proportional calculation
        var proportionalDays = (annualDays / 12.0) * monthsRemaining;

        // Round UP to full days per Art. 155¹ KP
        var result = (int)Math.Ceiling(proportionalDays);

        _logger.LogInformation(
            "Calculated proportional vacation: {Result} days for employment starting {StartDate} ({MonthsRemaining} months)",
            result, employmentStartDate.ToString("yyyy-MM-dd"), monthsRemaining);

        return result;
    }

    /// <summary>
    /// Checks if user can take vacation on specified dates.
    /// Validates available vacation days based on leave type and usage.
    /// </summary>
    public async Task<(bool CanTake, string? ErrorMessage)> CanTakeVacationAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate,
        LeaveType leaveType)
    {
        // 1. Get user
        var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found for vacation validation", userId);
            return (false, "Użytkownik nie istnieje");
        }

        // 2. Calculate requested days (business days only)
        var requestedDays = CalculateBusinessDays(startDate, endDate);

        if (requestedDays <= 0)
        {
            return (false, "Nieprawidłowy zakres dat urlopu");
        }

        // 3. Validate based on leave type
        switch (leaveType)
        {
            case LeaveType.OnDemand:
                // On-demand vacation: max 4 days per year (Polish law)
                if (user.OnDemandVacationDaysUsed >= 4)
                {
                    _logger.LogInformation(
                        "User {UserId} exhausted on-demand vacation (4/4 used)",
                        userId);
                    return (false, "Wykorzystano już wszystkie 4 dni urlopu na żądanie w tym roku");
                }

                var remainingOnDemand = 4 - user.OnDemandVacationDaysUsed;
                if (requestedDays > remainingOnDemand)
                {
                    _logger.LogInformation(
                        "User {UserId} requesting {RequestedDays} on-demand days but only {Remaining} available",
                        userId, requestedDays, remainingOnDemand);
                    return (false, $"Możesz wziąć jeszcze {remainingOnDemand} dni urlopu na żądanie");
                }
                break;

            case LeaveType.Circumstantial:
                // Circumstantial leave: typically 2 days per event (Polish law)
                if (requestedDays > 2)
                {
                    _logger.LogInformation(
                        "User {UserId} requesting {RequestedDays} circumstantial days (max 2)",
                        userId, requestedDays);
                    return (false, "Urlop okolicznościowy to maksymalnie 2 dni na wydarzenie");
                }
                break;

            case LeaveType.Annual:
                // Annual vacation: check total available (current year + carried over)
                var availableDays = user.TotalAvailableVacationDays;

                if (requestedDays > availableDays)
                {
                    _logger.LogInformation(
                        "User {UserId} requesting {RequestedDays} annual vacation days but only {Available} available",
                        userId, requestedDays, availableDays);
                    return (false, $"Brak wystarczającej liczby dni urlopu. Dostępne: {availableDays} dni");
                }
                break;

            case LeaveType.Sick:
                // Sick leave: always allowed (cannot be rejected per Polish law)
                _logger.LogDebug("Sick leave validation - always allowed per Polish law");
                break;

            default:
                _logger.LogWarning("Unknown leave type {LeaveType} for user {UserId}", leaveType, userId);
                return (false, "Nieznany typ urlopu");
        }

        _logger.LogInformation(
            "Vacation validation passed for user {UserId}: {LeaveType}, {RequestedDays} days ({StartDate} - {EndDate})",
            userId, leaveType, requestedDays, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

        return (true, null);
    }

    /// <summary>
    /// Calculates total number of vacation days used by user in specified year.
    /// Sums up all approved vacation schedules for the year.
    /// </summary>
    public async Task<int> CalculateVacationDaysUsedAsync(Guid userId, int year)
    {
        var allVacations = await _unitOfWork.VacationScheduleRepository.GetAllAsync();

        var totalDays = allVacations
            .Where(v => v.UserId == userId &&
                       (v.StartDate.Year == year || v.EndDate.Year == year) &&
                       (v.Status == Domain.Enums.VacationStatus.Completed ||
                        v.Status == Domain.Enums.VacationStatus.Active))
            .Sum(v => v.DaysCount);

        _logger.LogDebug(
            "Calculated vacation days used for user {UserId} in {Year}: {TotalDays} days",
            userId, year, totalDays);

        return totalDays;
    }

    /// <summary>
    /// Gets total available vacation days for user (current year + carried over).
    /// Uses computed property from User entity.
    /// </summary>
    public async Task<int> GetAvailableVacationDaysAsync(Guid userId)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);

        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found when getting available vacation days", userId);
            return 0;
        }

        var available = user.TotalAvailableVacationDays;

        _logger.LogDebug(
            "User {UserId} has {Available} vacation days available (Annual: {Annual}, Used: {Used}, Carried: {Carried})",
            userId, available, user.AnnualVacationDays, user.VacationDaysUsed, user.CarriedOverVacationDays);

        return available;
    }

    /// <summary>
    /// Calculates number of business days between two dates (excluding weekends).
    /// Includes both start and end date.
    /// Note: Does NOT exclude Polish public holidays - this can be extended later.
    /// </summary>
    public int CalculateBusinessDays(DateTime startDate, DateTime endDate)
    {
        if (endDate < startDate)
        {
            _logger.LogWarning(
                "Invalid date range: end date {EndDate} before start date {StartDate}",
                endDate, startDate);
            return 0;
        }

        var businessDays = 0;
        var currentDate = startDate.Date;

        while (currentDate <= endDate.Date)
        {
            // Exclude Saturdays and Sundays
            if (currentDate.DayOfWeek != DayOfWeek.Saturday
                && currentDate.DayOfWeek != DayOfWeek.Sunday)
            {
                businessDays++;
            }

            currentDate = currentDate.AddDays(1);
        }

        _logger.LogDebug(
            "Calculated {BusinessDays} business days between {StartDate} and {EndDate}",
            businessDays, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

        return businessDays;
    }
}
