using System.Text;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Interfaces;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Services;

/// <summary>
/// Service for managing audit logs throughout the application.
/// Used for tracking important actions for security, compliance, and troubleshooting.
/// </summary>
public class AuditLogService : IAuditLogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(
        IUnitOfWork unitOfWork,
        ILogger<AuditLogService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new audit log entry.
    /// </summary>
    public async Task LogActionAsync(
        string entityType,
        string entityId,
        string action,
        Guid? userId = null,
        string? oldValue = null,
        string? newValue = null,
        string? reason = null,
        string? ipAddress = null)
    {
        // Guard clauses
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentNullException(nameof(entityType));

        if (string.IsNullOrWhiteSpace(entityId))
            throw new ArgumentNullException(nameof(entityId));

        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentNullException(nameof(action));

        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            UserId = userId,
            OldValue = oldValue,
            NewValue = newValue,
            Reason = reason,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        };

        await _unitOfWork.AuditLogRepository.CreateAsync(auditLog);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Audit log created: {EntityType} {EntityId} - {Action} by User {UserId}",
            entityType, entityId, action, userId?.ToString() ?? "System");
    }

    /// <summary>
    /// Gets audit logs with optional filtering.
    /// </summary>
    public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(
        string? entityType = null,
        string? action = null,
        Guid? userId = null,
        DateTime? from = null,
        DateTime? to = null)
    {
        var allLogs = await _unitOfWork.AuditLogRepository.GetAllAsync();

        // Apply filters
        var filtered = allLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            filtered = filtered.Where(l => l.EntityType == entityType);
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            filtered = filtered.Where(l => l.Action == action);
        }

        if (userId.HasValue)
        {
            filtered = filtered.Where(l => l.UserId == userId.Value);
        }

        if (from.HasValue)
        {
            filtered = filtered.Where(l => l.Timestamp >= from.Value);
        }

        if (to.HasValue)
        {
            filtered = filtered.Where(l => l.Timestamp <= to.Value);
        }

        var result = filtered
            .OrderByDescending(l => l.Timestamp)
            .ToList();

        _logger.LogDebug(
            "Retrieved {Count} audit logs with filters: EntityType={EntityType}, Action={Action}, UserId={UserId}",
            result.Count, entityType, action, userId);

        return result;
    }

    /// <summary>
    /// Gets audit logs for a specific entity.
    /// </summary>
    public async Task<IEnumerable<AuditLog>> GetEntityAuditHistoryAsync(string entityType, string entityId)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentNullException(nameof(entityType));

        if (string.IsNullOrWhiteSpace(entityId))
            throw new ArgumentNullException(nameof(entityId));

        var allLogs = await _unitOfWork.AuditLogRepository.GetAllAsync();

        var entityLogs = allLogs
            .Where(l => l.EntityType == entityType && l.EntityId == entityId)
            .OrderByDescending(l => l.Timestamp)
            .ToList();

        _logger.LogDebug(
            "Retrieved {Count} audit logs for {EntityType} {EntityId}",
            entityLogs.Count, entityType, entityId);

        return entityLogs;
    }

    /// <summary>
    /// Exports audit logs to CSV format.
    /// </summary>
    public async Task<byte[]> ExportAuditLogToCsvAsync(
        string? entityType = null,
        string? action = null,
        Guid? userId = null,
        DateTime? from = null,
        DateTime? to = null)
    {
        var logs = await GetAuditLogsAsync(entityType, action, userId, from, to);

        var csv = new StringBuilder();

        // Header
        csv.AppendLine("Timestamp,EntityType,EntityId,Action,UserId,UserName,OldValue,NewValue,Reason,IpAddress");

        // Data rows
        foreach (var log in logs)
        {
            var userName = log.User != null
                ? $"{log.User.FirstName} {log.User.LastName}"
                : "System";

            csv.AppendLine(
                $"\"{log.Timestamp:yyyy-MM-dd HH:mm:ss}\"," +
                $"\"{EscapeCsv(log.EntityType)}\"," +
                $"\"{EscapeCsv(log.EntityId)}\"," +
                $"\"{EscapeCsv(log.Action)}\"," +
                $"\"{log.UserId?.ToString() ?? ""}\"," +
                $"\"{EscapeCsv(userName)}\"," +
                $"\"{EscapeCsv(log.OldValue)}\"," +
                $"\"{EscapeCsv(log.NewValue)}\"," +
                $"\"{EscapeCsv(log.Reason)}\"," +
                $"\"{EscapeCsv(log.IpAddress)}\"");
        }

        _logger.LogInformation(
            "Exported {Count} audit logs to CSV",
            logs.Count());

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    /// <summary>
    /// Escapes special characters in CSV fields.
    /// </summary>
    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // Escape double quotes by doubling them
        return value.Replace("\"", "\"\"");
    }
}
