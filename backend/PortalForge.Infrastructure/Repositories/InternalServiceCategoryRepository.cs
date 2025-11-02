using Microsoft.EntityFrameworkCore;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Infrastructure.Persistence;

namespace PortalForge.Infrastructure.Repositories;

public class InternalServiceCategoryRepository : IInternalServiceCategoryRepository
{
    private readonly ApplicationDbContext _context;

    public InternalServiceCategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InternalServiceCategory?> GetByIdAsync(Guid id)
    {
        return await _context.InternalServiceCategories
            .Include(c => c.Services)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<InternalServiceCategory>> GetAllAsync()
    {
        return await _context.InternalServiceCategories
            .Include(c => c.Services.Where(s => s.IsActive))
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(InternalServiceCategory category)
    {
        await _context.InternalServiceCategories.AddAsync(category);
        return category.Id;
    }

    public async Task UpdateAsync(InternalServiceCategory category)
    {
        _context.InternalServiceCategories.Update(category);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var category = await _context.InternalServiceCategories.FindAsync(id);
        if (category != null)
        {
            _context.InternalServiceCategories.Remove(category);
        }
    }
}
