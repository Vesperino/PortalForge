using PortalForge.Domain.Entities;

namespace PortalForge.Application.Common.Interfaces;

public interface IAuditLogRepository
{
    Task<AuditLog?> GetByIdAsync(Guid id);
    Task<IEnumerable<AuditLog>> GetAllAsync();
    Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<AuditLog>> GetByActionAsync(string action);
    Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<Guid> CreateAsync(AuditLog auditLog);

    /// <summary>
    /// Gets audit logs with filtering and pagination.
    /// </summary>
    /// <param name="entityType">Optional entity type filter</param>
    /// <param name="action">Optional action filter</param>
    /// <param name="userId">Optional user ID filter</param>
    /// <param name="fromDate">Optional start date filter</param>
    /// <param name="toDate">Optional end date filter</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Tuple containing total count and page items</returns>
    Task<(int TotalCount, List<AuditLog> Items)> GetFilteredAsync(
        string? entityType,
        string? action,
        Guid? userId,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize);
}

