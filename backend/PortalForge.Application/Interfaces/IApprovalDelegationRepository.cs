using PortalForge.Domain.Entities;

namespace PortalForge.Application.Interfaces;

public interface IApprovalDelegationRepository
{
    Task<ApprovalDelegation?> GetByIdAsync(Guid id);
    Task<IEnumerable<ApprovalDelegation>> GetAllAsync();
    Task<IEnumerable<ApprovalDelegation>> GetActiveByUserIdAsync(Guid userId);
    Task<IEnumerable<ApprovalDelegation>> GetActiveDelegationsFromUserAsync(Guid fromUserId);
    Task<IEnumerable<ApprovalDelegation>> GetActiveDelegationsToUserAsync(Guid toUserId);
    Task AddAsync(ApprovalDelegation delegation);
    Task UpdateAsync(ApprovalDelegation delegation);
    Task DeleteAsync(Guid id);
}