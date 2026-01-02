using PortalForge.Domain.Entities;

namespace PortalForge.Application.Common.Interfaces;

public interface IRoleGroupPermissionRepository
{
    Task<IEnumerable<RoleGroupPermission>> GetByRoleGroupIdAsync(Guid roleGroupId);
    Task<IEnumerable<RoleGroupPermission>> GetByRoleGroupIdsAsync(IEnumerable<Guid> roleGroupIds);
    Task<IEnumerable<RoleGroupPermission>> GetByPermissionIdAsync(Guid permissionId);
    Task CreateAsync(RoleGroupPermission roleGroupPermission);
    Task DeleteAsync(Guid roleGroupId, Guid permissionId);
    Task DeleteByRoleGroupIdAsync(Guid roleGroupId);
}

