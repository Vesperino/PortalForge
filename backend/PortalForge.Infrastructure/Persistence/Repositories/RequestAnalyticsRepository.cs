using Microsoft.EntityFrameworkCore;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Persistence.Repositories;

public class RequestAnalyticsRepository : IRequestAnalyticsRepository
{
    private readonly ApplicationDbContext _context;

    public RequestAnalyticsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RequestAnalytics?> GetByIdAsync(Guid id)
    {
        return await _context.RequestAnalytics
            .Include(ra => ra.User)
            .FirstOrDefaultAsync(ra => ra.Id == id);
    }

    public async Task<IEnumerable<RequestAnalytics>> GetAllAsync()
    {
        return await _context.RequestAnalytics
            .Include(ra => ra.User)
            .ToListAsync();
    }

    public async Task<IEnumerable<RequestAnalytics>> GetByUserIdAsync(Guid userId)
    {
        return await _context.RequestAnalytics
            .Include(ra => ra.User)
            .Where(ra => ra.UserId == userId)
            .OrderByDescending(ra => ra.Year)
            .ThenByDescending(ra => ra.Month)
            .ToListAsync();
    }

    public async Task<RequestAnalytics?> GetByUserAndPeriodAsync(Guid userId, int year, int month)
    {
        return await _context.RequestAnalytics
            .Include(ra => ra.User)
            .FirstOrDefaultAsync(ra => ra.UserId == userId && ra.Year == year && ra.Month == month);
    }

    public async Task<IEnumerable<RequestAnalytics>> GetByPeriodAsync(int year, int month)
    {
        return await _context.RequestAnalytics
            .Include(ra => ra.User)
            .Where(ra => ra.Year == year && ra.Month == month)
            .ToListAsync();
    }

    public async Task<IEnumerable<RequestAnalytics>> GetByYearAsync(int year)
    {
        return await _context.RequestAnalytics
            .Include(ra => ra.User)
            .Where(ra => ra.Year == year)
            .OrderBy(ra => ra.Month)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(RequestAnalytics analytics)
    {
        _context.RequestAnalytics.Add(analytics);
        await _context.SaveChangesAsync();
        return analytics.Id;
    }

    public async Task UpdateAsync(RequestAnalytics analytics)
    {
        _context.RequestAnalytics.Update(analytics);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var analytics = await _context.RequestAnalytics.FindAsync(id);
        if (analytics != null)
        {
            _context.RequestAnalytics.Remove(analytics);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteByUserAndPeriodAsync(Guid userId, int year, int month)
    {
        var analytics = await _context.RequestAnalytics
            .FirstOrDefaultAsync(ra => ra.UserId == userId && ra.Year == year && ra.Month == month);
        
        if (analytics != null)
        {
            _context.RequestAnalytics.Remove(analytics);
            await _context.SaveChangesAsync();
        }
    }
}