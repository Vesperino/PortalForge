using Microsoft.EntityFrameworkCore;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Infrastructure.Persistence;

namespace PortalForge.Infrastructure.Repositories;

public class InternalServiceRepository : IInternalServiceRepository
{
    private readonly ApplicationDbContext _context;

    public InternalServiceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InternalService?> GetByIdAsync(Guid id)
    {
        return await _context.InternalServices
            .Include(s => s.Category)
            .Include(s => s.CreatedBy)
            .Include(s => s.ServiceDepartments)
                .ThenInclude(sd => sd.Department)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<InternalService>> GetAllAsync()
    {
        return await _context.InternalServices
            .Include(s => s.Category)
            .Include(s => s.CreatedBy)
            .Include(s => s.ServiceDepartments)
                .ThenInclude(sd => sd.Department)
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<InternalService>> GetActiveAsync()
    {
        return await _context.InternalServices
            .Include(s => s.Category)
            .Include(s => s.ServiceDepartments)
                .ThenInclude(sd => sd.Department)
            .Where(s => s.IsActive)
            .OrderBy(s => s.IsPinned ? 0 : 1)
            .ThenBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<InternalService>> GetGlobalServicesAsync()
    {
        return await _context.InternalServices
            .Include(s => s.Category)
            .Where(s => s.IsActive && s.IsGlobal)
            .OrderBy(s => s.IsPinned ? 0 : 1)
            .ThenBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<InternalService>> GetByDepartmentIdAsync(Guid departmentId)
    {
        return await _context.InternalServices
            .Include(s => s.Category)
            .Include(s => s.ServiceDepartments)
            .Where(s => s.IsActive &&
                   (s.IsGlobal || s.ServiceDepartments.Any(sd => sd.DepartmentId == departmentId)))
            .OrderBy(s => s.IsGlobal ? 0 : 1)
            .ThenBy(s => s.IsPinned ? 0 : 1)
            .ThenBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<InternalService>> GetByDepartmentIdsAsync(List<Guid> departmentIds)
    {
        return await _context.InternalServices
            .Include(s => s.Category)
            .Include(s => s.ServiceDepartments)
            .Where(s => s.IsActive &&
                   (s.IsGlobal || s.ServiceDepartments.Any(sd => departmentIds.Contains(sd.DepartmentId))))
            .OrderBy(s => s.IsGlobal ? 0 : 1)
            .ThenBy(s => s.IsPinned ? 0 : 1)
            .ThenBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<InternalService>> GetByCategoryIdAsync(Guid categoryId)
    {
        return await _context.InternalServices
            .Include(s => s.Category)
            .Include(s => s.ServiceDepartments)
                .ThenInclude(sd => sd.Department)
            .Where(s => s.CategoryId == categoryId)
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(InternalService service)
    {
        await _context.InternalServices.AddAsync(service);
        return service.Id;
    }

    public async Task UpdateAsync(InternalService service)
    {
        _context.InternalServices.Update(service);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var service = await _context.InternalServices
            .Include(s => s.ServiceDepartments)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (service != null)
        {
            _context.InternalServices.Remove(service);
        }
    }

    public async Task AssignToDepartmentsAsync(Guid serviceId, List<Guid> departmentIds)
    {
        // Remove existing assignments
        var existingAssignments = await _context.InternalServiceDepartments
            .Where(sd => sd.InternalServiceId == serviceId)
            .ToListAsync();

        _context.InternalServiceDepartments.RemoveRange(existingAssignments);

        // Add new assignments
        var newAssignments = departmentIds.Select(deptId => new InternalServiceDepartment
        {
            InternalServiceId = serviceId,
            DepartmentId = deptId,
            AssignedAt = DateTime.UtcNow
        }).ToList();

        await _context.InternalServiceDepartments.AddRangeAsync(newAssignments);
    }

    public async Task RemoveDepartmentAssignmentsAsync(Guid serviceId)
    {
        var assignments = await _context.InternalServiceDepartments
            .Where(sd => sd.InternalServiceId == serviceId)
            .ToListAsync();

        _context.InternalServiceDepartments.RemoveRange(assignments);
    }
}
