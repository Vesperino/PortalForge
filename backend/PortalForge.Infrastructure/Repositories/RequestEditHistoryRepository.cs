using Microsoft.EntityFrameworkCore;
using PortalForge.Application.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Infrastructure.Persistence;

namespace PortalForge.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for managing request edit history.
/// </summary>
public class RequestEditHistoryRepository : IRequestEditHistoryRepository
{
    private readonly ApplicationDbContext _context;

    public RequestEditHistoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RequestEditHistory?> GetByIdAsync(Guid id)
    {
        return await _context.RequestEditHistories
            .Include(e => e.EditedBy)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<RequestEditHistory>> GetByRequestIdAsync(Guid requestId)
    {
        return await _context.RequestEditHistories
            .Include(e => e.EditedBy)
            .Where(e => e.RequestId == requestId)
            .OrderByDescending(e => e.EditedAt)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(RequestEditHistory editHistory)
    {
        _context.RequestEditHistories.Add(editHistory);
        return editHistory.Id;
    }

    public async Task DeleteAsync(Guid id)
    {
        var editHistory = await _context.RequestEditHistories.FindAsync(id);
        if (editHistory != null)
        {
            _context.RequestEditHistories.Remove(editHistory);
        }
    }
}
