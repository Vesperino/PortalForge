using Microsoft.EntityFrameworkCore;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Infrastructure.Persistence;

namespace PortalForge.Infrastructure.Repositories;

public class UserRoleGroupRepository : IUserRoleGroupRepository
{
    private readonly ApplicationDbContext _context;

    public UserRoleGroupRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserRoleGroup>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserRoleGroups
            .Include(urg => urg.RoleGroup)
            .ThenInclude(rg => rg.RoleGroupPermissions)
            .ThenInclude(rgp => rgp.Permission)
            .Where(urg => urg.UserId == userId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserRoleGroup>> GetByUserIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        var userIdList = userIds.ToList();
        return await _context.UserRoleGroups
            .Include(urg => urg.RoleGroup)
            .Where(urg => userIdList.Contains(urg.UserId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserRoleGroup>> GetByRoleGroupIdAsync(Guid roleGroupId, CancellationToken cancellationToken = default)
    {
        return await _context.UserRoleGroups
            .Include(urg => urg.User)
            .Where(urg => urg.RoleGroupId == roleGroupId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserRoleGroup>> GetByRoleGroupIdsAsync(IEnumerable<Guid> roleGroupIds, CancellationToken cancellationToken = default)
    {
        var roleGroupIdList = roleGroupIds.ToList();
        return await _context.UserRoleGroups
            .Where(urg => roleGroupIdList.Contains(urg.RoleGroupId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(UserRoleGroup userRoleGroup, CancellationToken cancellationToken = default)
    {
        await _context.UserRoleGroups.AddAsync(userRoleGroup, cancellationToken);
    }

    public async Task DeleteAsync(Guid userId, Guid roleGroupId, CancellationToken cancellationToken = default)
    {
        var userRoleGroup = await _context.UserRoleGroups
            .FirstOrDefaultAsync(urg => urg.UserId == userId && urg.RoleGroupId == roleGroupId, cancellationToken);

        if (userRoleGroup != null)
        {
            _context.UserRoleGroups.Remove(userRoleGroup);
        }
    }

    public async Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userRoleGroups = await _context.UserRoleGroups
            .Where(urg => urg.UserId == userId)
            .ToListAsync(cancellationToken);

        _context.UserRoleGroups.RemoveRange(userRoleGroups);
    }
}

