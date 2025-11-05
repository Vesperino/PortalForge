using Microsoft.EntityFrameworkCore;
using PortalForge.Application.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Infrastructure.Persistence;

namespace PortalForge.Infrastructure.Repositories;

public class ApprovalDelegationRepository : IApprovalDelegationRepository
{
    private readonly ApplicationDbContext _context;

    public ApprovalDelegationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApprovalDelegation?> GetByIdAsync(Guid id)
    {
        return await _context.ApprovalDelegations
            .Include(d => d.FromUser)
            .Include(d => d.ToUser)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<IEnumerable<ApprovalDelegation>> GetAllAsync()
    {
        return await _context.ApprovalDelegations
            .Include(d => d.FromUser)
            .Include(d => d.ToUser)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<ApprovalDelegation>> GetActiveByUserIdAsync(Guid userId)
    {
        var currentDate = DateTime.UtcNow;
        
        return await _context.ApprovalDelegations
            .Include(d => d.FromUser)
            .Include(d => d.ToUser)
            .Where(d => (d.FromUserId == userId || d.ToUserId == userId) &&
                       d.IsActive &&
                       d.StartDate <= currentDate &&
                       (d.EndDate == null || d.EndDate >= currentDate))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<ApprovalDelegation>> GetActiveDelegationsFromUserAsync(Guid fromUserId)
    {
        var currentDate = DateTime.UtcNow;
        
        return await _context.ApprovalDelegations
            .Include(d => d.FromUser)
            .Include(d => d.ToUser)
            .Where(d => d.FromUserId == fromUserId &&
                       d.IsActive &&
                       d.StartDate <= currentDate &&
                       (d.EndDate == null || d.EndDate >= currentDate))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<ApprovalDelegation>> GetActiveDelegationsToUserAsync(Guid toUserId)
    {
        var currentDate = DateTime.UtcNow;
        
        return await _context.ApprovalDelegations
            .Include(d => d.FromUser)
            .Include(d => d.ToUser)
            .Where(d => d.ToUserId == toUserId &&
                       d.IsActive &&
                       d.StartDate <= currentDate &&
                       (d.EndDate == null || d.EndDate >= currentDate))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(ApprovalDelegation delegation)
    {
        await _context.ApprovalDelegations.AddAsync(delegation);
    }

    public async Task UpdateAsync(ApprovalDelegation delegation)
    {
        _context.ApprovalDelegations.Update(delegation);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var delegation = await _context.ApprovalDelegations.FindAsync(id);
        if (delegation != null)
        {
            _context.ApprovalDelegations.Remove(delegation);
        }
    }
}