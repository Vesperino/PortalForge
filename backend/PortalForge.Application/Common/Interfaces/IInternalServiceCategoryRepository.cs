using PortalForge.Domain.Entities;

namespace PortalForge.Application.Common.Interfaces;

/// <summary>
/// Repository interface for InternalServiceCategory entity operations.
/// </summary>
public interface IInternalServiceCategoryRepository
{
    Task<InternalServiceCategory?> GetByIdAsync(Guid id);
    Task<IEnumerable<InternalServiceCategory>> GetAllAsync();
    Task<Guid> CreateAsync(InternalServiceCategory category);
    Task UpdateAsync(InternalServiceCategory category);
    Task DeleteAsync(Guid id);
}
