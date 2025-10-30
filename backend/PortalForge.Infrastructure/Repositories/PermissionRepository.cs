using Microsoft.EntityFrameworkCore;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Infrastructure.Persistence;

namespace PortalForge.Infrastructure.Repositories;

public class PermissionRepository : IPermissionRepository
{
    private readonly ApplicationDbContext _context;

    public PermissionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Permission?> GetByIdAsync(Guid id)
    {
        return await _context.Permissions
            .Include(p => p.RoleGroupPermissions)
            .ThenInclude(rgp => rgp.RoleGroup)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Permission?> GetByNameAsync(string name)
    {
        return await _context.Permissions
            .FirstOrDefaultAsync(p => p.Name == name);
    }

    public async Task<IEnumerable<Permission>> GetAllAsync()
    {
        return await _context.Permissions
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Permission>> GetByCategoryAsync(string category)
    {
        return await _context.Permissions
            .Where(p => p.Category == category)
            .OrderBy(p => p.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(Permission permission)
    {
        await _context.Permissions.AddAsync(permission);
        return permission.Id;
    }

    public async Task UpdateAsync(Permission permission)
    {
        _context.Permissions.Update(permission);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var permission = await _context.Permissions.FindAsync(id);
        if (permission != null)
        {
            _context.Permissions.Remove(permission);
        }
    }
}

