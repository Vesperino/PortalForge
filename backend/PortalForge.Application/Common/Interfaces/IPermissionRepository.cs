using PortalForge.Domain.Entities;

namespace PortalForge.Application.Common.Interfaces;

public interface IPermissionRepository
{
    Task<Permission?> GetByIdAsync(Guid id);
    Task<Permission?> GetByNameAsync(string name);
    Task<IEnumerable<Permission>> GetAllAsync();
    Task<IEnumerable<Permission>> GetByCategoryAsync(string category);

    Task<(IEnumerable<Permission> Permissions, int TotalCount)> GetFilteredAsync(
        string? searchTerm,
        string? category,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any permissions exist in the database.
    /// Used by seed handlers to avoid loading all records.
    /// </summary>
    Task<bool> AnyAsync();

    Task<Guid> CreateAsync(Permission permission);
    Task UpdateAsync(Permission permission);
    Task DeleteAsync(Guid id);
}

