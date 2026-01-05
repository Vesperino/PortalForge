using PortalForge.Domain.Entities;

namespace PortalForge.Application.Common.Interfaces;

public interface IPositionRepository
{
    Task<Position?> GetByIdAsync(Guid id);
    Task<Position?> GetByNameAsync(string name);
    Task<IEnumerable<Position>> GetAllAsync();
    Task<IEnumerable<Position>> GetActiveAsync();
    Task<IEnumerable<Position>> SearchByNameAsync(string searchTerm);

    Task<(IEnumerable<Position> Positions, int TotalCount)> GetFilteredAsync(
        string? searchTerm,
        bool? isActive,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(Position position);
    Task UpdateAsync(Position position);
    Task DeleteAsync(Guid id);
}
