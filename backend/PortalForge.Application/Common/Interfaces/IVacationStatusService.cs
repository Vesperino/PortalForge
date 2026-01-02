namespace PortalForge.Application.Common.Interfaces;

/// <summary>
/// Service responsible for managing vacation and sick leave status updates.
/// Handles activation and completion of vacations based on dates.
/// </summary>
public interface IVacationStatusService
{
    /// <summary>
    /// Daily job: Updates vacation statuses based on current date.
    /// - Scheduled -> Active (if StartDate &lt;= today)
    /// - Active -> Completed (if EndDate &lt; today)
    /// Sends notifications to substitutes when vacations start/end.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateVacationStatusesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes approved sick leave (L4) requests by creating SickLeave records.
    /// Called by background job to automatically convert approved sick leave requests
    /// into SickLeave entities for tracking and compliance.
    /// Sends notifications if ZUS documentation is required (>33 days).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ProcessApprovedSickLeaveRequestsAsync(CancellationToken cancellationToken = default);
}
