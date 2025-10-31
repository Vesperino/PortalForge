using Microsoft.EntityFrameworkCore;
using PortalForge.Application.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Infrastructure.Persistence;

namespace PortalForge.Infrastructure.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly ApplicationDbContext _context;

    public DepartmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Department?> GetByIdAsync(Guid id)
    {
        return await _context.Departments
            .Include(d => d.ParentDepartment)
            .Include(d => d.ChildDepartments)
            .Include(d => d.HeadOfDepartment)
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<IEnumerable<Department>> GetAllAsync()
    {
        return await _context.Departments
            .Include(d => d.ParentDepartment)
            .Include(d => d.HeadOfDepartment)
            .Where(d => d.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<Department>> GetRootDepartmentsAsync()
    {
        return await _context.Departments
            .Include(d => d.HeadOfDepartment)
            .Include(d => d.ChildDepartments)
            .Where(d => d.ParentDepartmentId == null && d.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<Department>> GetChildDepartmentsAsync(Guid parentId)
    {
        return await _context.Departments
            .Include(d => d.HeadOfDepartment)
            .Include(d => d.ChildDepartments)
            .Where(d => d.ParentDepartmentId == parentId && d.IsActive)
            .ToListAsync();
    }

    public async Task<Department> CreateAsync(Department department)
    {
        department.CreatedAt = DateTime.UtcNow;
        await _context.Departments.AddAsync(department);
        await _context.SaveChangesAsync();
        return department;
    }

    public async Task UpdateAsync(Department department)
    {
        department.UpdatedAt = DateTime.UtcNow;
        _context.Departments.Update(department);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department != null)
        {
            department.IsActive = false;
            department.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
