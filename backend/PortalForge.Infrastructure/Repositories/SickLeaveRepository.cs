using Microsoft.EntityFrameworkCore;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;
using PortalForge.Infrastructure.Persistence;

namespace PortalForge.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for managing sick leave (L4) records.
/// </summary>
public class SickLeaveRepository : ISickLeaveRepository
{
    private readonly ApplicationDbContext _context;

    public SickLeaveRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SickLeave?> GetByIdAsync(Guid id)
    {
        return await _context.SickLeaves
            .Include(sl => sl.User)
            .Include(sl => sl.SourceRequest)
            .FirstOrDefaultAsync(sl => sl.Id == id);
    }

    public async Task<IEnumerable<SickLeave>> GetAllAsync()
    {
        return await _context.SickLeaves
            .Include(sl => sl.User)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(SickLeave sickLeave)
    {
        _context.SickLeaves.Add(sickLeave);
        return sickLeave.Id;
    }

    public async Task UpdateAsync(SickLeave sickLeave)
    {
        _context.SickLeaves.Update(sickLeave);
    }

    public async Task DeleteAsync(Guid id)
    {
        var sickLeave = await _context.SickLeaves.FindAsync(id);
        if (sickLeave != null)
        {
            _context.SickLeaves.Remove(sickLeave);
        }
    }

    public async Task<List<SickLeave>> GetByUserIdAsync(Guid userId)
    {
        return await _context.SickLeaves
            .Where(sl => sl.UserId == userId)
            .OrderByDescending(sl => sl.StartDate)
            .ToListAsync();
    }

    public async Task<List<SickLeave>> GetByUserAndDateRangeAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate)
    {
        return await _context.SickLeaves
            .Where(sl => sl.UserId == userId &&
                        sl.EndDate >= startDate &&
                        sl.StartDate <= endDate)
            .OrderBy(sl => sl.StartDate)
            .ToListAsync();
    }

    public async Task<List<SickLeave>> GetActiveAsync()
    {
        var now = DateTime.UtcNow.Date;

        return await _context.SickLeaves
            .Include(sl => sl.User)
            .Where(sl => sl.Status == SickLeaveStatus.Active &&
                        sl.StartDate <= now &&
                        sl.EndDate >= now)
            .ToListAsync();
    }

    public async Task<List<SickLeave>> GetRequiringZusDocumentAsync()
    {
        return await _context.SickLeaves
            .Include(sl => sl.User)
            .Where(sl => sl.RequiresZusDocument && string.IsNullOrEmpty(sl.ZusDocumentUrl))
            .OrderBy(sl => sl.StartDate)
            .ToListAsync();
    }
}
