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

    public async Task<RequestTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.RequestTemplates
            .Include(rt => rt.Fields)
            .Include(rt => rt.ApprovalStepTemplates)
                .ThenInclude(ast => ast.QuizQuestions)
            .Include(rt => rt.CreatedBy)
            .FirstOrDefaultAsync(rt => rt.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<RequestTemplate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.RequestTemplates
            .Include(rt => rt.Fields)
            .Include(rt => rt.ApprovalStepTemplates)
                .ThenInclude(ast => ast.QuizQuestions)
            .Include(rt => rt.CreatedBy)
            .OrderByDescending(rt => rt.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<RequestTemplate>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.RequestTemplates
            .Include(rt => rt.CreatedBy)
            .Where(rt => rt.IsActive)
            .OrderBy(rt => rt.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<RequestTemplate>> GetByDepartmentAsync(string? departmentId, CancellationToken cancellationToken = default)
    {
        return await _context.RequestTemplates
            .Include(rt => rt.CreatedBy)
            .Where(rt => rt.IsActive && rt.DepartmentId == departmentId)
            .OrderBy(rt => rt.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<RequestTemplate>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        return await _context.RequestTemplates
            .Include(rt => rt.CreatedBy)
            .Where(rt => rt.IsActive && rt.Category == category)
            .OrderBy(rt => rt.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<RequestTemplate>> GetAvailableForUserAsync(string? userDepartment, CancellationToken cancellationToken = default)
    {
        return await _context.RequestTemplates
            .Include(rt => rt.CreatedBy)
            .Where(rt => rt.IsActive && (rt.DepartmentId == null || rt.DepartmentId == userDepartment))
            .OrderBy(rt => rt.Category)
            .ThenBy(rt => rt.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid> CreateAsync(RequestTemplate template, CancellationToken cancellationToken = default)
    {
        await _context.RequestTemplates.AddAsync(template, cancellationToken);
        return template.Id;
    }

    public async Task UpdateAsync(RequestTemplate template, CancellationToken cancellationToken = default)
    {
        _context.RequestTemplates.Update(template);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var template = await _context.RequestTemplates.FindAsync(new object[] { id }, cancellationToken);
        if (template != null)
        {
            _context.RequestTemplates.Remove(template);
        }
    }
}

