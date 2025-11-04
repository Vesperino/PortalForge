using MediatR;
using PortalForge.Application.DTOs;

namespace PortalForge.Application.UseCases.Admin.Queries.GetAuditLogs;

/// <summary>
/// Query to retrieve audit logs with filtering and pagination.
/// Used for security audits, compliance, and troubleshooting.
/// </summary>
public class GetAuditLogsQuery : IRequest<PagedResult<AuditLogDto>>
{
    /// <summary>
    /// Filter by entity type (e.g., "User", "Request", "VacationSchedule").
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// Filter by action (e.g., "VacationAllowanceUpdated", "RequestCancelled").
    /// </summary>
    public string? Action { get; set; }

    /// <summary>
    /// Filter by user who performed the action.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Filter by minimum timestamp.
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Filter by maximum timestamp.
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Page number (1-based).
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page (max 100).
    /// </summary>
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// DTO for audit log entry in API responses.
/// </summary>
public class AuditLogDto
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string? UserFullName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Reason { get; set; }
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
}
