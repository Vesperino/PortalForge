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

    public async Task<(IEnumerable<Permission> Permissions, int TotalCount)> GetFilteredAsync(
        string? searchTerm,
        string? category,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Permissions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                p.Description.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category == category);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var permissions = await query
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (permissions, totalCount);
    }

    public async Task<bool> AnyAsync()
    {
        return await _context.Permissions.AnyAsync();
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

