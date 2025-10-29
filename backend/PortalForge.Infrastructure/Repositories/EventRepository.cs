using Microsoft.EntityFrameworkCore;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Infrastructure.Persistence;

namespace PortalForge.Infrastructure.Repositories;

public class EventRepository : IEventRepository
{
    private readonly ApplicationDbContext _context;

    public EventRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Event?> GetByIdAsync(int id)
    {
        return await _context.Events
            .Include(e => e.Creator)
            .Include(e => e.News)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Event>> GetAllAsync()
    {
        return await _context.Events
            .Include(e => e.Creator)
            .Include(e => e.News)
            .OrderBy(e => e.StartDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Event>> GetUpcomingAsync(DateTime fromDate, int? limit = null)
    {
        var query = _context.Events
            .Include(e => e.Creator)
            .Include(e => e.News)
            .Where(e => e.StartDate >= fromDate)
            .OrderBy(e => e.StartDate)
            .AsNoTracking();

        if (limit.HasValue)
        {
            query = (IQueryable<Event>)query.Take(limit.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Event>> GetByMonthAsync(int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        return await _context.Events
            .Include(e => e.Creator)
            .Include(e => e.News)
            .Where(e => e.StartDate >= startDate && e.StartDate < endDate)
            .OrderBy(e => e.StartDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Event>> GetByTagAsync(EventTag tag)
    {
        return await _context.Events
            .Include(e => e.Creator)
            .Include(e => e.News)
            .Where(e => e.Tags.Contains(tag))
            .OrderBy(e => e.StartDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> CreateAsync(Event @event)
    {
        await _context.Events.AddAsync(@event);
        return @event.Id;
    }

    public async Task UpdateAsync(Event @event)
    {
        _context.Events.Update(@event);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var @event = await _context.Events.FindAsync(id);
        if (@event != null)
        {
            _context.Events.Remove(@event);
        }
    }
}
