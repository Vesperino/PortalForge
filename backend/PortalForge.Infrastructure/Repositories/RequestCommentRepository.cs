using Microsoft.EntityFrameworkCore;
using PortalForge.Application.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Infrastructure.Persistence;

namespace PortalForge.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for managing request comments.
/// </summary>
public class RequestCommentRepository : IRequestCommentRepository
{
    private readonly ApplicationDbContext _context;

    public RequestCommentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RequestComment?> GetByIdAsync(Guid id)
    {
        return await _context.RequestComments
            .Include(c => c.User)
            .Include(c => c.Request)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<RequestComment>> GetByRequestIdAsync(Guid requestId)
    {
        return await _context.RequestComments
            .Include(c => c.User)
            .Where(c => c.RequestId == requestId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(RequestComment comment)
    {
        _context.RequestComments.Add(comment);
        return comment.Id;
    }

    public async Task UpdateAsync(RequestComment comment)
    {
        _context.RequestComments.Update(comment);
    }

    public async Task DeleteAsync(Guid id)
    {
        var comment = await _context.RequestComments.FindAsync(id);
        if (comment != null)
        {
            _context.RequestComments.Remove(comment);
        }
    }
}
