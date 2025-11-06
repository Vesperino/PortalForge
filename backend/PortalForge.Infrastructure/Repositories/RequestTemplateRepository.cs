using Microsoft.EntityFrameworkCore;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Infrastructure.Persistence;

namespace PortalForge.Infrastructure.Repositories;

public class RequestTemplateRepository : IRequestTemplateRepository
{
    private readonly ApplicationDbContext _context;

    public RequestTemplateRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RequestTemplate?> GetByIdAsync(Guid id)
    {
        return await _context.RequestTemplates
            .Include(rt => rt.Fields)
            .Include(rt => rt.ApprovalStepTemplates)
                .ThenInclude(ast => ast.QuizQuestions)
            .Include(rt => rt.CreatedBy)
            .FirstOrDefaultAsync(rt => rt.Id == id);
    }

    public async Task<IEnumerable<RequestTemplate>> GetAllAsync()
    {
        return await _context.RequestTemplates
            .Include(rt => rt.CreatedBy)
            .OrderByDescending(rt => rt.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<RequestTemplate>> GetActiveAsync()
    {
        return await _context.RequestTemplates
            .Include(rt => rt.CreatedBy)
            .Where(rt => rt.IsActive)
            .OrderBy(rt => rt.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<RequestTemplate>> GetByDepartmentAsync(string? departmentId)
    {
        return await _context.RequestTemplates
            .Include(rt => rt.CreatedBy)
            .Where(rt => rt.IsActive && rt.DepartmentId == departmentId)
            .OrderBy(rt => rt.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<RequestTemplate>> GetByCategoryAsync(string category)
    {
        return await _context.RequestTemplates
            .Include(rt => rt.CreatedBy)
            .Where(rt => rt.IsActive && rt.Category == category)
            .OrderBy(rt => rt.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<RequestTemplate>> GetAvailableForUserAsync(string? userDepartment)
    {
        return await _context.RequestTemplates
            .Include(rt => rt.CreatedBy)
            .Where(rt => rt.IsActive && (rt.DepartmentId == null || rt.DepartmentId == userDepartment))
            .OrderBy(rt => rt.Category)
            .ThenBy(rt => rt.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(RequestTemplate template)
    {
        await _context.RequestTemplates.AddAsync(template);
        return template.Id;
    }

    public async Task UpdateAsync(RequestTemplate template)
    {
        _context.RequestTemplates.Update(template);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var template = await _context.RequestTemplates.FindAsync(id);
        if (template != null)
        {
            _context.RequestTemplates.Remove(template);
        }
    }
}

