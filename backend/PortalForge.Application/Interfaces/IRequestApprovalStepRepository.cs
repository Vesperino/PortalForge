using PortalForge.Domain.Entities;

namespace PortalForge.Application.Interfaces;

public interface IRequestApprovalStepRepository
{
    Task<RequestApprovalStep?> GetByIdAsync(Guid id);
    Task<IEnumerable<RequestApprovalStep>> GetAllAsync();
    Task<IEnumerable<RequestApprovalStep>> GetByRequestIdAsync(Guid requestId);
    Task<IEnumerable<RequestApprovalStep>> GetByApproverIdAsync(Guid approverId);
    Task<IEnumerable<RequestApprovalStep>> GetPendingByApproverIdAsync(Guid approverId);
    Task<IEnumerable<RequestApprovalStep>> GetByParallelGroupIdAsync(string parallelGroupId, Guid requestId);
    Task AddAsync(RequestApprovalStep step);
    Task UpdateAsync(RequestApprovalStep step);
    Task DeleteAsync(Guid id);
}