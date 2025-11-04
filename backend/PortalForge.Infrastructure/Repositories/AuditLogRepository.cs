using Microsoft.EntityFrameworkCore;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Infrastructure.Persistence;

namespace PortalForge.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly ApplicationDbContext _context;

    public AuditLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AuditLog?> GetByIdAsync(Guid id)
    {
        return await _context.AuditLogs
            .Include(al => al.User)
            .FirstOrDefaultAsync(al => al.Id == id);
    }

    public async Task<IEnumerable<AuditLog>> GetAllAsync()
    {
        return await _context.AuditLogs
            .Include(al => al.User)
            .OrderByDescending(al => al.Timestamp)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId)
    {
        return await _context.AuditLogs
            .Where(al => al.UserId == userId)
            .OrderByDescending(al => al.Timestamp)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByActionAsync(string action)
    {
        return await _context.AuditLogs
            .Include(al => al.User)
            .Where(al => al.Action == action)
            .OrderByDescending(al => al.Timestamp)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.AuditLogs
            .Include(al => al.User)
            .Where(al => al.Timestamp >= startDate && al.Timestamp <= endDate)
            .OrderByDescending(al => al.Timestamp)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(AuditLog auditLog)
    {
        await _context.AuditLogs.AddAsync(auditLog);
        return auditLog.Id;
    }

    public async Task<(int TotalCount, List<AuditLog> Items)> GetFilteredAsync(
        string? entityType,
        string? action,
        Guid? userId,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize)
    {
        var query = _context.AuditLogs
            .Include(al => al.User)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(entityType))
        {
            query = query.Where(al => al.EntityType == entityType);
        }

        if (!string.IsNullOrEmpty(action))
        {
            query = query.Where(al => al.Action == action);
        }

        if (userId.HasValue)
        {
            query = query.Where(al => al.UserId == userId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(al => al.Timestamp >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(al => al.Timestamp <= toDate.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply pagination and ordering
        var items = await query
            .OrderByDescending(al => al.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return (totalCount, items);
    }
}

