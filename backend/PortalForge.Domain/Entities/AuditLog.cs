namespace PortalForge.Domain.Entities;

/// <summary>
/// Generic audit log for tracking important actions in the system.
/// Used for security, compliance, and troubleshooting purposes.
/// </summary>
public class AuditLog
{
    /// <summary>
    /// Unique identifier for this audit log entry.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Type of entity being audited (e.g., "User", "Request", "VacationSchedule").
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// ID of the specific entity instance that was affected.
    /// Stored as string to accommodate different ID types.
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Action that was performed (e.g., "VacationAllowanceUpdated", "RequestCancelled", "DepartmentTransfer").
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// ID of the user who performed the action. Null for system-generated actions.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Navigation property to the user who performed the action.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// The value before the change (optional, JSON or plain text).
    /// </summary>
    public string? OldValue { get; set; }

    /// <summary>
    /// The value after the change (optional, JSON or plain text).
    /// </summary>
    public string? NewValue { get; set; }

    /// <summary>
    /// Reason for the action, if provided by the user.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// When this action occurred.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// IP address of the client that initiated the action (for security auditing).
    /// </summary>
    public string? IpAddress { get; set; }
}
