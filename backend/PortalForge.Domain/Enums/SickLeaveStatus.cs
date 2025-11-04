namespace PortalForge.Domain.Enums;

/// <summary>
/// Status of a sick leave (L4) record.
/// </summary>
public enum SickLeaveStatus
{
    /// <summary>
    /// Sick leave is currently active (employee is on sick leave).
    /// </summary>
    Active = 0,

    /// <summary>
    /// Sick leave has been completed (end date has passed).
    /// </summary>
    Completed = 1,

    /// <summary>
    /// Sick leave was cancelled by administrator or system.
    /// </summary>
    Cancelled = 2
}
