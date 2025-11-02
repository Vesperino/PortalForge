namespace PortalForge.Domain.Enums;

/// <summary>
/// Represents the current status of a vacation schedule.
/// </summary>
public enum VacationStatus
{
    /// <summary>
    /// Vacation is scheduled but hasn't started yet (StartDate > today).
    /// </summary>
    Scheduled,

    /// <summary>
    /// Vacation is currently active (StartDate <= today <= EndDate).
    /// </summary>
    Active,

    /// <summary>
    /// Vacation has ended (EndDate < today).
    /// </summary>
    Completed,

    /// <summary>
    /// Vacation was cancelled before it started.
    /// </summary>
    Cancelled
}
