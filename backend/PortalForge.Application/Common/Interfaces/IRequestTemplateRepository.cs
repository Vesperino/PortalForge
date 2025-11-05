using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.Common.Interfaces;

/// <summary>
/// Repository interface for RequestTemplate entity operations.
/// </summary>
public interface IRequestTemplateRepository
{
    Task<RequestTemplate?> GetByIdAsync(Guid id);
    Task<IEnumerable<RequestTemplate>> GetAllAsync();
    Task<IEnumerable<RequestTemplate>> GetActiveAsync();
    Task<IEnumerable<RequestTemplate>> GetByDepartmentAsync(string? departmentId);
    Task<IEnumerable<RequestTemplate>> GetByCategoryAsync(string category);
    Task<IEnumerable<RequestTemplate>> GetAvailableForUserAsync(string? userDepartment);
    
    /// <summary>
    /// Gets templates by service category for service request routing
    /// </summary>
    /// <param name="serviceCategory">Service category to filter by</param>
    /// <returns>List of templates in the specified service category</returns>
    Task<IEnumerable<RequestTemplate>> GetByServiceCategoryAsync(string serviceCategory);
    
    /// <summary>
    /// Gets templates that support specific field types
    /// </summary>
    /// <param name="fieldTypes">List of field types to search for</param>
    /// <returns>List of templates containing any of the specified field types</returns>
    Task<IEnumerable<RequestTemplate>> GetByFieldTypesAsync(List<FieldType> fieldTypes);
    
    /// <summary>
    /// Gets templates with advanced form features (conditional logic, validation rules, etc.)
    /// </summary>
    /// <returns>List of templates with enhanced features</returns>
    Task<IEnumerable<RequestTemplate>> GetAdvancedTemplatesAsync();
    
    Task<Guid> CreateAsync(RequestTemplate template);
    Task UpdateAsync(RequestTemplate template);
    Task DeleteAsync(Guid id);
}

