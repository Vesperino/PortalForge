using PortalForge.Domain.Entities;

namespace PortalForge.Application.Interfaces;

/// <summary>
/// Repository interface for OrganizationalPermission entity
/// </summary>
public interface IOrganizationalPermissionRepository
{
    /// <summary>
    /// Get organizational permission by user ID
    /// </summary>
    Task<OrganizationalPermission?> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Create a new organizational permission
    /// </summary>
    Task<Guid> CreateAsync(OrganizationalPermission permission);

    /// <summary>
    /// Update an existing organizational permission
    /// </summary>
    Task UpdateAsync(OrganizationalPermission permission);

    /// <summary>
    /// Delete an organizational permission
    /// </summary>
    Task DeleteAsync(Guid id);
}
