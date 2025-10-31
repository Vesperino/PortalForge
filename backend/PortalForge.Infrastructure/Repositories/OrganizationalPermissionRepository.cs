using Microsoft.EntityFrameworkCore;
using PortalForge.Application.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Infrastructure.Persistence;

namespace PortalForge.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for OrganizationalPermission entity
/// </summary>
public class OrganizationalPermissionRepository : IOrganizationalPermissionRepository
{
    private readonly ApplicationDbContext _context;

    public OrganizationalPermissionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get organizational permission by user ID
    /// </summary>
    public async Task<OrganizationalPermission?> GetByUserIdAsync(Guid userId)
    {
        return await _context.OrganizationalPermissions
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    /// <summary>
    /// Create a new organizational permission
    /// </summary>
    public async Task<Guid> CreateAsync(OrganizationalPermission permission)
    {
        _context.OrganizationalPermissions.Add(permission);
        await _context.SaveChangesAsync();
        return permission.Id;
    }

    /// <summary>
    /// Update an existing organizational permission
    /// </summary>
    public async Task UpdateAsync(OrganizationalPermission permission)
    {
        _context.OrganizationalPermissions.Update(permission);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Delete an organizational permission
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        var permission = await _context.OrganizationalPermissions.FindAsync(id);
        if (permission != null)
        {
            _context.OrganizationalPermissions.Remove(permission);
            await _context.SaveChangesAsync();
        }
    }
}
