using PortalForge.Domain.Entities;

namespace PortalForge.Application.Common.Interfaces;

public interface IRoleGroupRepository
{
    Task<RoleGroup?> GetByIdAsync(Guid id);
    Task<RoleGroup?> GetByNameAsync(string name);
    Task<IEnumerable<RoleGroup>> GetAllAsync();
    Task<IEnumerable<RoleGroup>> GetSystemRolesAsync();
    Task<IEnumerable<User>> GetUsersInGroupAsync(Guid roleGroupId);
    Task<Guid> CreateAsync(RoleGroup roleGroup);
    Task UpdateAsync(RoleGroup roleGroup);
    Task DeleteAsync(Guid id);
}

