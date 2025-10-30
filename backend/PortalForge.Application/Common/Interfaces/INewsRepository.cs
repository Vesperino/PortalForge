using PortalForge.Domain.Entities;

namespace PortalForge.Application.Common.Interfaces;

/// <summary>
/// Repository interface for News entity operations.
/// </summary>
public interface INewsRepository
{
    Task<News?> GetByIdAsync(int id);
    Task<IEnumerable<News>> GetAllAsync();
    Task<IEnumerable<News>> GetByCategoryAsync(NewsCategory category);
    Task<IEnumerable<News>> GetLatestAsync(int count);
    Task<IEnumerable<News>> GetByDepartmentAsync(int departmentId);
    Task<IEnumerable<News>> GetEventsAsync();
    Task<IEnumerable<News>> GetUpcomingEventsAsync(DateTime fromDate);
    Task<IEnumerable<News>> GetByHashtagsAsync(List<string> hashtags);
    Task<int> CreateAsync(News news);
    Task UpdateAsync(News news);
    Task DeleteAsync(int id);
    Task IncrementViewsAsync(int id);
}
