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

    public async Task<IEnumerable<RoleGroup>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        var idList = ids.ToList();
        return await _context.RoleGroups
            .Where(rg => idList.Contains(rg.Id))
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

    public async Task<(IEnumerable<RoleGroup> RoleGroups, int TotalCount)> GetFilteredAsync(
        string? searchTerm,
        bool? isSystemRole,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.RoleGroups.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(rg =>
                rg.Name.ToLower().Contains(term) ||
                rg.Description.ToLower().Contains(term));
        }

        if (isSystemRole.HasValue)
        {
            query = query.Where(rg => rg.IsSystemRole == isSystemRole.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var roleGroups = await query
            .OrderBy(rg => rg.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (roleGroups, totalCount);
    }

    public async Task<bool> AnyAsync()
    {
        return await _context.RoleGroups.AnyAsync();
    }

    public async Task<IEnumerable<User>> GetUsersInGroupAsync(Guid roleGroupId)
    {
        return await _context.UserRoleGroups
            .Where(urg => urg.RoleGroupId == roleGroupId)
            .Include(urg => urg.User)
            .Select(urg => urg.User)
            .Where(u => u.IsActive)
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

