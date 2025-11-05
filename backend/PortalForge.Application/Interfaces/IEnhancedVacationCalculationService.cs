using PortalForge.Application.DTOs;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.Interfaces;

/// <summary>
/// Enhanced vacation calculation service that extends the base IVacationCalculationService
/// with Polish labor law compliance features, conflict detection, and advanced validation.
/// </summary>
public interface IEnhancedVacationCalculationService : IVacationCalculationService
{
    /// <summary>
    /// Validates circumstantial leave request according to Polish labor law.
    /// Checks documentation requirements, valid reasons, and day limits.
    /// </summary>
    /// <param name="userId">ID of the user requesting leave</param>
    /// <param name="startDate">Leave start date</param>
    /// <param name="endDate">Leave end date</param>
    /// <param name="reason">Reason for circumstantial leave</param>
    /// <param name="hasDocumentation">Whether supporting documentation is provided</param>
    /// <returns>Validation result with success status and details</returns>
    Task<CircumstantialLeaveResult> ValidateCircumstantialLeaveAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate,
        string reason,
        bool hasDocumentation);

    /// <summary>
    /// Validates on-demand vacation request according to Polish labor law.
    /// Checks the 4-day annual limit and ensures proper usage.
    /// </summary>
    /// <param name="userId">ID of the user requesting vacation</param>
    /// <param name="startDate">Vacation start date</param>
    /// <param name="endDate">Vacation end date</param>
    /// <returns>Validation result with success status and remaining days</returns>
    Task<OnDemandVacationResult> ValidateOnDemandVacationAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate);

    /// <summary>
    /// Checks for vacation conflicts with existing schedules.
    /// Detects overlapping vacations and team coverage issues.
    /// </summary>
    /// <param name="userId">ID of the user requesting vacation</param>
    /// <param name="startDate">Vacation start date</param>
    /// <param name="endDate">Vacation end date</param>
    /// <returns>Conflict detection result with details</returns>
    Task<VacationConflictResult> CheckVacationConflictsAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate);

    /// <summary>
    /// Gets the remaining on-demand vacation days for a user in the specified year.
    /// Polish law allows maximum 4 days per year.
    /// </summary>
    /// <param name="userId">ID of the user</param>
    /// <param name="year">Year to check (defaults to current year)</param>
    /// <returns>Number of remaining on-demand vacation days</returns>
    Task<int> GetRemainingOnDemandDaysAsync(Guid userId, int year = 0);
}