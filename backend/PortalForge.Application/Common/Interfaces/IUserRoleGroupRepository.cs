using PortalForge.Domain.Entities;

namespace PortalForge.Application.Common.Interfaces;

public interface IUserRoleGroupRepository
{
    Task<IEnumerable<UserRoleGroup>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<UserRoleGroup>> GetByUserIdsAsync(IEnumerable<Guid> userIds);
    Task<IEnumerable<UserRoleGroup>> GetByRoleGroupIdAsync(Guid roleGroupId);
    Task<IEnumerable<UserRoleGroup>> GetByRoleGroupIdsAsync(IEnumerable<Guid> roleGroupIds);
    Task CreateAsync(UserRoleGroup userRoleGroup);
    Task DeleteAsync(Guid userId, Guid roleGroupId);
    Task DeleteByUserIdAsync(Guid userId);
}

