using PortalForge.Domain.Entities;

namespace PortalForge.Application.Common.Interfaces;

public interface IUserRoleGroupRepository
{
    Task<IEnumerable<UserRoleGroup>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserRoleGroup>> GetByUserIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserRoleGroup>> GetByRoleGroupIdAsync(Guid roleGroupId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserRoleGroup>> GetByRoleGroupIdsAsync(IEnumerable<Guid> roleGroupIds, CancellationToken cancellationToken = default);
    Task CreateAsync(UserRoleGroup userRoleGroup, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid userId, Guid roleGroupId, CancellationToken cancellationToken = default);
    Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

