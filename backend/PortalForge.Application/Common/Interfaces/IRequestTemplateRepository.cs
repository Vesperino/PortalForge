using PortalForge.Domain.Entities;

namespace PortalForge.Application.Common.Interfaces;

/// <summary>
/// Repository interface for RequestTemplate entity operations.
/// </summary>
public interface IRequestTemplateRepository
{
    Task<RequestTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<RequestTemplate>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<RequestTemplate>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<RequestTemplate>> GetByDepartmentAsync(string? departmentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<RequestTemplate>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<IEnumerable<RequestTemplate>> GetAvailableForUserAsync(string? userDepartment, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(RequestTemplate template, CancellationToken cancellationToken = default);
    Task UpdateAsync(RequestTemplate template, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

