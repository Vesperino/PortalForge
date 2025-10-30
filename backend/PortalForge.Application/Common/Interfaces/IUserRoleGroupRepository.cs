using PortalForge.Domain.Entities;

namespace PortalForge.Application.Common.Interfaces;

public interface IUserRoleGroupRepository
{
    Task<IEnumerable<UserRoleGroup>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<UserRoleGroup>> GetByRoleGroupIdAsync(Guid roleGroupId);
    Task CreateAsync(UserRoleGroup userRoleGroup);
    Task DeleteAsync(Guid userId, Guid roleGroupId);
    Task DeleteByUserIdAsync(Guid userId);
}

