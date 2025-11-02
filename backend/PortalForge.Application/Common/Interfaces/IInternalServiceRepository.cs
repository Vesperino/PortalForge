using PortalForge.Domain.Entities;

namespace PortalForge.Application.Common.Interfaces;

/// <summary>
/// Repository interface for InternalService entity operations.
/// </summary>
public interface IInternalServiceRepository
{
    Task<InternalService?> GetByIdAsync(Guid id);
    Task<IEnumerable<InternalService>> GetAllAsync();
    Task<IEnumerable<InternalService>> GetActiveAsync();
    Task<IEnumerable<InternalService>> GetGlobalServicesAsync();
    Task<IEnumerable<InternalService>> GetByDepartmentIdAsync(Guid departmentId);
    Task<IEnumerable<InternalService>> GetByDepartmentIdsAsync(List<Guid> departmentIds);
    Task<IEnumerable<InternalService>> GetByCategoryIdAsync(Guid categoryId);
    Task<Guid> CreateAsync(InternalService service);
    Task UpdateAsync(InternalService service);
    Task DeleteAsync(Guid id);
    Task AssignToDepartmentsAsync(Guid serviceId, List<Guid> departmentIds);
    Task RemoveDepartmentAssignmentsAsync(Guid serviceId);
}
