using Microsoft.EntityFrameworkCore;
using PortalForge.Application.Interfaces;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Persistence.Repositories;

public class CachedLocationRepository : ICachedLocationRepository
{
    private readonly ApplicationDbContext _context;

    public CachedLocationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CachedLocation?> GetByIdAsync(int id)
    {
        return await _context.CachedLocations.FindAsync(id);
    }

    public async Task<IEnumerable<CachedLocation>> GetAllAsync()
    {
        return await _context.CachedLocations
            .OrderBy(l => l.Type)
            .ThenBy(l => l.Name)
            .ToListAsync();
    }

    public async Task<CachedLocation?> SearchByAddressOrNameAsync(string searchTerm)
    {
        return await _context.CachedLocations
            .Where(l => l.Address.Contains(searchTerm) || l.Name.Contains(searchTerm))
            .FirstOrDefaultAsync();
    }

    public async Task<int> CreateAsync(CachedLocation location)
    {
        _context.CachedLocations.Add(location);
        await _context.SaveChangesAsync();
        return location.Id;
    }

    public async Task DeleteAsync(int id)
    {
        var location = await _context.CachedLocations.FindAsync(id);
        if (location != null)
        {
            _context.CachedLocations.Remove(location);
            await _context.SaveChangesAsync();
        }
    }
}
