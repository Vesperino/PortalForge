using PortalForge.Application.DTOs;
using PortalForge.Domain.Entities;

namespace PortalForge.Application.Common.Interfaces;

/// <summary>
/// Service responsible for managing vacation substitutes and team calendar.
/// Handles finding active substitutes and generating team vacation views.
/// </summary>
public interface IVacationSubstituteService
{
    /// <summary>
    /// Gets the active substitute for a user if they're currently on vacation.
    /// Returns null if user is not on vacation right now.
    /// </summary>
    /// <param name="userId">ID of the user to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The substitute user if on vacation, otherwise null.</returns>
    Task<User?> GetActiveSubstituteAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets team vacation calendar with statistics and conflict alerts for a date range.
    /// Includes all vacations, team size, coverage alerts, and statistical summary.
    /// </summary>
    /// <param name="departmentId">Department to get calendar for.</param>
    /// <param name="startDate">Start of date range (inclusive).</param>
    /// <param name="endDate">End of date range (inclusive).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Complete vacation calendar with stats and alerts.</returns>
    Task<VacationCalendar> GetTeamCalendarAsync(
        Guid departmentId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets vacations where the specified user is the substitute.
    /// Useful for showing "My Substitutions" view.
    /// </summary>
    /// <param name="userId">ID of the substitute user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of vacations where user is covering.</returns>
    Task<List<VacationSchedule>> GetMySubstitutionsAsync(Guid userId, CancellationToken cancellationToken = default);
}
