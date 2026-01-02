using Microsoft.EntityFrameworkCore;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Infrastructure.Persistence;

namespace PortalForge.Infrastructure.Repositories;

public class RoleGroupPermissionRepository : IRoleGroupPermissionRepository
{
    private readonly ApplicationDbContext _context;

    public RoleGroupPermissionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RoleGroupPermission>> GetByRoleGroupIdAsync(Guid roleGroupId)
    {
        return await _context.RoleGroupPermissions
            .Include(rgp => rgp.Permission)
            .Where(rgp => rgp.RoleGroupId == roleGroupId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<RoleGroupPermission>> GetByRoleGroupIdsAsync(IEnumerable<Guid> roleGroupIds)
    {
        var roleGroupIdList = roleGroupIds.ToList();
        return await _context.RoleGroupPermissions
            .Include(rgp => rgp.Permission)
            .Where(rgp => roleGroupIdList.Contains(rgp.RoleGroupId))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<RoleGroupPermission>> GetByPermissionIdAsync(Guid permissionId)
    {
        return await _context.RoleGroupPermissions
            .Include(rgp => rgp.RoleGroup)
            .Where(rgp => rgp.PermissionId == permissionId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task CreateAsync(RoleGroupPermission roleGroupPermission)
    {
        await _context.RoleGroupPermissions.AddAsync(roleGroupPermission);
    }

    public async Task DeleteAsync(Guid roleGroupId, Guid permissionId)
    {
        var roleGroupPermission = await _context.RoleGroupPermissions
            .FirstOrDefaultAsync(rgp => rgp.RoleGroupId == roleGroupId && rgp.PermissionId == permissionId);
        
        if (roleGroupPermission != null)
        {
            _context.RoleGroupPermissions.Remove(roleGroupPermission);
        }
    }

    public async Task DeleteByRoleGroupIdAsync(Guid roleGroupId)
    {
        var roleGroupPermissions = await _context.RoleGroupPermissions
            .Where(rgp => rgp.RoleGroupId == roleGroupId)
            .ToListAsync();
        
        _context.RoleGroupPermissions.RemoveRange(roleGroupPermissions);
    }
}

