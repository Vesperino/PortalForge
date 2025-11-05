using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.Common.Interfaces;

/// <summary>
/// Repository interface for Request entity operations.
/// </summary>
public interface IRequestRepository
{
    Task<Request?> GetByIdAsync(Guid id);
    Task<IEnumerable<Request>> GetAllAsync();
    Task<IEnumerable<Request>> GetBySubmitterAsync(Guid submitterId);
    Task<IEnumerable<Request>> GetByApproverAsync(Guid approverId);
    Task<IEnumerable<Request>> GetByStatusAsync(RequestStatus status);
    Task<IEnumerable<Request>> GetByTemplateIdAsync(Guid templateId);
    Task<Request?> GetByRequestNumberAsync(string requestNumber);

    /// <summary>
    /// Gets all pending requests for a specific user (where user is the submitter).
    /// Used for employee transfers to reassign approval steps to new supervisor.
    /// </summary>
    /// <param name="userId">User ID of the submitter</param>
    /// <returns>List of pending requests with approval steps included</returns>
    Task<IEnumerable<Request>> GetPendingRequestsByUserAsync(Guid userId);

    Task<Guid> CreateAsync(Request request);
    Task UpdateAsync(Request request);
    Task DeleteAsync(Guid id);
}

