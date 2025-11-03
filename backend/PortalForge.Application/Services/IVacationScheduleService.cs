using PortalForge.Application.DTOs;
using PortalForge.Domain.Entities;

namespace PortalForge.Application.Services;

/// <summary>
/// Service responsible for managing vacation schedules and substitute routing.
/// Handles automatic vacation creation, conflict detection, and status updates.
/// </summary>
public interface IVacationScheduleService
{
    /// <summary>
    /// Creates vacation schedule from approved vacation request.
    /// Extracts start date, end date, and substitute from form data.
    /// Validates substitute is not the user and is an active employee.
    /// </summary>
    /// <param name="vacationRequest">The approved vacation request.</param>
    /// <exception cref="InvalidOperationException">If form data is invalid.</exception>
    /// <exception cref="ValidationException">If substitute is invalid (same as user, inactive, etc.).</exception>
    /// <exception cref="NotFoundException">If substitute user not found.</exception>
    Task CreateFromApprovedRequestAsync(Request vacationRequest);

    /// <summary>
    /// Gets the active substitute for a user if they're currently on vacation.
    /// Returns null if user is not on vacation right now.
    /// </summary>
    /// <param name="userId">ID of the user to check.</param>
    /// <returns>The substitute user if on vacation, otherwise null.</returns>
    Task<User?> GetActiveSubstituteAsync(Guid userId);

    /// <summary>
    /// Gets team vacation calendar with statistics and conflict alerts for a date range.
    /// Includes all vacations, team size, coverage alerts, and statistical summary.
    /// </summary>
    /// <param name="departmentId">Department to get calendar for.</param>
    /// <param name="startDate">Start of date range (inclusive).</param>
    /// <param name="endDate">End of date range (inclusive).</param>
    /// <returns>Complete vacation calendar with stats and alerts.</returns>
    Task<VacationCalendar> GetTeamCalendarAsync(
        Guid departmentId,
        DateTime startDate,
        DateTime endDate);

    /// <summary>
    /// Gets vacations where the specified user is the substitute.
    /// Useful for showing "My Substitutions" view.
    /// </summary>
    /// <param name="userId">ID of the substitute user.</param>
    /// <returns>List of vacations where user is covering.</returns>
    Task<List<VacationSchedule>> GetMySubstitutionsAsync(Guid userId);

    /// <summary>
    /// Daily job: Updates vacation statuses based on current date.
    /// - Scheduled → Active (if StartDate &lt;= today)
    /// - Active → Completed (if EndDate &lt; today)
    /// Sends notifications to substitutes when vacations start/end.
    /// </summary>
    Task UpdateVacationStatusesAsync();

    /// <summary>
    /// Processes approved sick leave (L4) requests by creating SickLeave records.
    /// Called by background job to automatically convert approved sick leave requests
    /// into SickLeave entities for tracking and compliance.
    /// Sends notifications if ZUS documentation is required (>33 days).
    /// </summary>
    Task ProcessApprovedSickLeaveRequestsAsync();
}
