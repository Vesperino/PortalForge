using PortalForge.Domain.Entities;

namespace PortalForge.Application.Interfaces;

public interface ICachedLocationRepository
{
    Task<CachedLocation?> GetByIdAsync(int id);
    Task<IEnumerable<CachedLocation>> GetAllAsync();
    Task<CachedLocation?> SearchByAddressOrNameAsync(string searchTerm);
    Task<int> CreateAsync(CachedLocation location);
    Task DeleteAsync(int id);
}
