using PortalForge.Domain.Enums;

namespace PortalForge.Application.Interfaces;

/// <summary>
/// Service for calculating vacation allowances and validating vacation availability.
/// Implements Polish labor law requirements (Kodeks Pracy 2025).
/// </summary>
public interface IVacationCalculationService
{
    /// <summary>
    /// Calculates proportional vacation days based on employment start date.
    /// According to Polish law (Art. 155¹ KP): employee gets proportional vacation
    /// based on months worked in the first year.
    /// </summary>
    /// <param name="employmentStartDate">Date when employee started working</param>
    /// <param name="annualDays">Total annual vacation days (default: uses configured VacationSettings.DefaultAnnualDays)</param>
    /// <returns>Number of vacation days employee is entitled to (rounded up to full days)</returns>
    int CalculateProportionalVacationDays(DateTime employmentStartDate, int annualDays = 0);

    /// <summary>
    /// Checks if user can take vacation on specified dates.
    /// Validates available vacation days based on leave type and usage.
    /// </summary>
    /// <param name="userId">ID of the user requesting vacation</param>
    /// <param name="startDate">Vacation start date</param>
    /// <param name="endDate">Vacation end date</param>
    /// <param name="leaveType">Type of leave being requested</param>
    /// <returns>Tuple with result (can take) and optional error message</returns>
    Task<(bool CanTake, string? ErrorMessage)> CanTakeVacationAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate,
        LeaveType leaveType);

    /// <summary>
    /// Calculates total number of vacation days used by user in specified year.
    /// </summary>
    /// <param name="userId">ID of the user</param>
    /// <param name="year">Year to calculate for</param>
    /// <returns>Total number of vacation days used</returns>
    Task<int> CalculateVacationDaysUsedAsync(Guid userId, int year);

    /// <summary>
    /// Gets total available vacation days for user (current year + carried over).
    /// </summary>
    /// <param name="userId">ID of the user</param>
    /// <returns>Total available vacation days</returns>
    Task<int> GetAvailableVacationDaysAsync(Guid userId);

    /// <summary>
    /// Calculates number of business days between two dates (excluding weekends).
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <returns>Number of business days</returns>
    int CalculateBusinessDays(DateTime startDate, DateTime endDate);
}
