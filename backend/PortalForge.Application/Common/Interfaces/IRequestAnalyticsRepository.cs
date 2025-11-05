using PortalForge.Domain.Entities;

namespace PortalForge.Application.Common.Interfaces;

/// <summary>
/// Repository interface for RequestAnalytics entity operations.
/// </summary>
public interface IRequestAnalyticsRepository
{
    Task<RequestAnalytics?> GetByIdAsync(Guid id);
    Task<IEnumerable<RequestAnalytics>> GetAllAsync();
    Task<IEnumerable<RequestAnalytics>> GetByUserIdAsync(Guid userId);
    Task<RequestAnalytics?> GetByUserAndPeriodAsync(Guid userId, int year, int month);
    Task<IEnumerable<RequestAnalytics>> GetByPeriodAsync(int year, int month);
    Task<IEnumerable<RequestAnalytics>> GetByYearAsync(int year);
    
    Task<Guid> CreateAsync(RequestAnalytics analytics);
    Task UpdateAsync(RequestAnalytics analytics);
    Task DeleteAsync(Guid id);
    Task DeleteByUserAndPeriodAsync(Guid userId, int year, int month);
}