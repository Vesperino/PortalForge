using Microsoft.EntityFrameworkCore;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Infrastructure.Persistence;

namespace PortalForge.Infrastructure.Repositories;

public class PositionRepository : IPositionRepository
{
    private readonly ApplicationDbContext _context;

    public PositionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Position?> GetByIdAsync(Guid id)
    {
        return await _context.Positions
            .Include(p => p.Users)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Position?> GetByNameAsync(string name)
    {
        return await _context.Positions
            .FirstOrDefaultAsync(p => p.Name.ToLower() == name.ToLower());
    }

    public async Task<IEnumerable<Position>> GetAllAsync()
    {
        return await _context.Positions
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Position>> GetActiveAsync()
    {
        return await _context.Positions
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Position>> SearchByNameAsync(string searchTerm)
    {
        var term = searchTerm.ToLower();
        return await _context.Positions
            .AsNoTracking()
            .Where(p => p.IsActive && p.Name.ToLower().Contains(term))
            .OrderBy(p => p.Name)
            .Take(10)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(Position position)
    {
        await _context.Positions.AddAsync(position);
        return position.Id;
    }

    public async Task UpdateAsync(Position position)
    {
        _context.Positions.Update(position);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var position = await _context.Positions.FindAsync(id);
        if (position != null)
        {
            _context.Positions.Remove(position);
        }
    }
}
