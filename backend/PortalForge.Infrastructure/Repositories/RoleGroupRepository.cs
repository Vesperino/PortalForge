using Microsoft.EntityFrameworkCore;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Infrastructure.Persistence;

namespace PortalForge.Infrastructure.Repositories;

public class RoleGroupRepository : IRoleGroupRepository
{
    private readonly ApplicationDbContext _context;

    public RoleGroupRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RoleGroup?> GetByIdAsync(Guid id)
    {
        return await _context.RoleGroups
            .Include(rg => rg.RoleGroupPermissions)
            .ThenInclude(rgp => rgp.Permission)
            .Include(rg => rg.UserRoleGroups)
            .ThenInclude(urg => urg.User)
            .FirstOrDefaultAsync(rg => rg.Id == id);
    }

    public async Task<RoleGroup?> GetByNameAsync(string name)
    {
        return await _context.RoleGroups
            .FirstOrDefaultAsync(rg => rg.Name == name);
    }

    public async Task<IEnumerable<RoleGroup>> GetAllAsync()
    {
        return await _context.RoleGroups
            .Include(rg => rg.RoleGroupPermissions)
            .ThenInclude(rgp => rgp.Permission)
            .OrderBy(rg => rg.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<RoleGroup>> GetSystemRolesAsync()
    {
        return await _context.RoleGroups
            .Where(rg => rg.IsSystemRole)
            .OrderBy(rg => rg.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(RoleGroup roleGroup)
    {
        await _context.RoleGroups.AddAsync(roleGroup);
        return roleGroup.Id;
    }

    public async Task UpdateAsync(RoleGroup roleGroup)
    {
        _context.RoleGroups.Update(roleGroup);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var roleGroup = await _context.RoleGroups.FindAsync(id);
        if (roleGroup != null)
        {
            _context.RoleGroups.Remove(roleGroup);
        }
    }
}

