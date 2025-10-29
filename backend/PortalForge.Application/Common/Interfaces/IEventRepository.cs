using PortalForge.Domain.Entities;

namespace PortalForge.Application.Common.Interfaces;

/// <summary>
/// Repository interface for Event entity operations.
/// </summary>
public interface IEventRepository
{
    Task<Event?> GetByIdAsync(int id);
    Task<IEnumerable<Event>> GetAllAsync();
    Task<IEnumerable<Event>> GetUpcomingAsync(DateTime fromDate, int? limit = null);
    Task<IEnumerable<Event>> GetByMonthAsync(int year, int month);
    Task<IEnumerable<Event>> GetByTagAsync(EventTag tag);
    Task<int> CreateAsync(Event @event);
    Task UpdateAsync(Event @event);
    Task DeleteAsync(int id);
}
