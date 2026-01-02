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

    public async Task<IEnumerable<UserRoleGroup>> GetByUserIdAsync(Guid userId)
    {
        return await _context.UserRoleGroups
            .Include(urg => urg.RoleGroup)
            .ThenInclude(rg => rg.RoleGroupPermissions)
            .ThenInclude(rgp => rgp.Permission)
            .Where(urg => urg.UserId == userId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<UserRoleGroup>> GetByUserIdsAsync(IEnumerable<Guid> userIds)
    {
        var userIdList = userIds.ToList();
        return await _context.UserRoleGroups
            .Include(urg => urg.RoleGroup)
            .Where(urg => userIdList.Contains(urg.UserId))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<UserRoleGroup>> GetByRoleGroupIdAsync(Guid roleGroupId)
    {
        return await _context.UserRoleGroups
            .Include(urg => urg.User)
            .Where(urg => urg.RoleGroupId == roleGroupId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<UserRoleGroup>> GetByRoleGroupIdsAsync(IEnumerable<Guid> roleGroupIds)
    {
        var roleGroupIdList = roleGroupIds.ToList();
        return await _context.UserRoleGroups
            .Where(urg => roleGroupIdList.Contains(urg.RoleGroupId))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task CreateAsync(UserRoleGroup userRoleGroup)
    {
        await _context.UserRoleGroups.AddAsync(userRoleGroup);
    }

    public async Task DeleteAsync(Guid userId, Guid roleGroupId)
    {
        var userRoleGroup = await _context.UserRoleGroups
            .FirstOrDefaultAsync(urg => urg.UserId == userId && urg.RoleGroupId == roleGroupId);
        
        if (userRoleGroup != null)
        {
            _context.UserRoleGroups.Remove(userRoleGroup);
        }
    }

    public async Task DeleteByUserIdAsync(Guid userId)
    {
        var userRoleGroups = await _context.UserRoleGroups
            .Where(urg => urg.UserId == userId)
            .ToListAsync();
        
        _context.UserRoleGroups.RemoveRange(userRoleGroups);
    }
}

