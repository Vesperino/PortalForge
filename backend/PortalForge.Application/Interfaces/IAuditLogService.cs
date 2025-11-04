using PortalForge.Domain.Entities;

namespace PortalForge.Application.Interfaces;

/// <summary>
/// Service for managing audit logs throughout the application.
/// Used for tracking important actions for security, compliance, and troubleshooting.
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Creates a new audit log entry.
    /// </summary>
    /// <param name="entityType">Type of entity (e.g., "User", "Request", "VacationSchedule")</param>
    /// <param name="entityId">ID of the affected entity</param>
    /// <param name="action">Action performed (e.g., "VacationAllowanceUpdated", "RequestCancelled")</param>
    /// <param name="userId">ID of user who performed the action (null for system actions)</param>
    /// <param name="oldValue">Value before the change (optional, JSON or plain text)</param>
    /// <param name="newValue">Value after the change (optional, JSON or plain text)</param>
    /// <param name="reason">Reason for the action (optional)</param>
    /// <param name="ipAddress">IP address of the client (optional)</param>
    Task LogActionAsync(
        string entityType,
        string entityId,
        string action,
        Guid? userId = null,
        string? oldValue = null,
        string? newValue = null,
        string? reason = null,
        string? ipAddress = null);

    /// <summary>
    /// Gets audit logs with optional filtering.
    /// </summary>
    /// <param name="entityType">Filter by entity type</param>
    /// <param name="action">Filter by action</param>
    /// <param name="userId">Filter by user ID</param>
    /// <param name="from">Filter by start date</param>
    /// <param name="to">Filter by end date</param>
    /// <returns>List of matching audit logs</returns>
    Task<IEnumerable<AuditLog>> GetAuditLogsAsync(
        string? entityType = null,
        string? action = null,
        Guid? userId = null,
        DateTime? from = null,
        DateTime? to = null);

    /// <summary>
    /// Gets audit logs for a specific entity.
    /// </summary>
    /// <param name="entityType">Type of entity</param>
    /// <param name="entityId">ID of the entity</param>
    /// <returns>List of audit logs for the entity, ordered by timestamp descending</returns>
    Task<IEnumerable<AuditLog>> GetEntityAuditHistoryAsync(string entityType, string entityId);

    /// <summary>
    /// Exports audit logs to CSV format.
    /// </summary>
    /// <param name="entityType">Filter by entity type</param>
    /// <param name="action">Filter by action</param>
    /// <param name="userId">Filter by user ID</param>
    /// <param name="from">Filter by start date</param>
    /// <param name="to">Filter by end date</param>
    /// <returns>CSV content as byte array</returns>
    Task<byte[]> ExportAuditLogToCsvAsync(
        string? entityType = null,
        string? action = null,
        Guid? userId = null,
        DateTime? from = null,
        DateTime? to = null);
}
