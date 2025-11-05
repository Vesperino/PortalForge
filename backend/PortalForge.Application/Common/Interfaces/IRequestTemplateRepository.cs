using PortalForge.Domain.Entities;

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
    Task<Guid> CreateAsync(RequestTemplate template);
    Task UpdateAsync(RequestTemplate template);
    Task DeleteAsync(Guid id);
}

