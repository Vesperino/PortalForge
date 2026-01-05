using PortalForge.Domain.Entities;

namespace PortalForge.Application.Common.Interfaces;

public interface IRoleGroupRepository
{
    Task<RoleGroup?> GetByIdAsync(Guid id);
    Task<RoleGroup?> GetByNameAsync(string name);
    Task<IEnumerable<RoleGroup>> GetAllAsync();
    Task<IEnumerable<RoleGroup>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task<IEnumerable<RoleGroup>> GetSystemRolesAsync();

    Task<(IEnumerable<RoleGroup> RoleGroups, int TotalCount)> GetFilteredAsync(
        string? searchTerm,
        bool? isSystemRole,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any role groups exist in the database.
    /// Used by seed handlers to avoid loading all records.
    /// </summary>
    Task<bool> AnyAsync();
    Task<IEnumerable<User>> GetUsersInGroupAsync(Guid roleGroupId);
    Task<Guid> CreateAsync(RoleGroup roleGroup);
    Task UpdateAsync(RoleGroup roleGroup);
    Task DeleteAsync(Guid id);
}

