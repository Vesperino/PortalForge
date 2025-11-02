using PortalForge.Domain.Entities;

namespace PortalForge.Application.Interfaces;

/// <summary>
/// Repository interface for Department entity operations.
/// </summary>
public interface IDepartmentRepository
{
    /// <summary>
    /// Gets a department by its ID, including navigation properties.
    /// </summary>
    Task<Department?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all departments.
    /// </summary>
    Task<IEnumerable<Department>> GetAllAsync();

    /// <summary>
    /// Gets all root departments (departments without a parent).
    /// </summary>
    Task<IEnumerable<Department>> GetRootDepartmentsAsync();

    /// <summary>
    /// Gets child departments of a specific department.
    /// </summary>
    Task<IEnumerable<Department>> GetChildDepartmentsAsync(Guid parentId);

    /// <summary>
    /// Creates a new department.
    /// </summary>
    Task<Department> CreateAsync(Department department);

    /// <summary>
    /// Updates an existing department.
    /// </summary>
    Task UpdateAsync(Department department);

    /// <summary>
    /// Soft deletes a department by setting IsActive to false.
    /// </summary>
    Task DeleteAsync(Guid id);
}
