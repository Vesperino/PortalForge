using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.Common.Interfaces;

/// <summary>
/// Repository interface for Request entity operations.
/// </summary>
public interface IRequestRepository
{
    Task<Request?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Request>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Request>> GetBySubmitterAsync(Guid submitterId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Request>> GetByApproverAsync(Guid approverId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Request>> GetByStatusAsync(RequestStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Request>> GetByTemplateIdAsync(Guid templateId, CancellationToken cancellationToken = default);
    Task<Request?> GetByRequestNumberAsync(string requestNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all pending requests for a specific user (where user is the submitter).
    /// Used for employee transfers to reassign approval steps to new supervisor.
    /// </summary>
    /// <param name="userId">User ID of the submitter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of pending requests with approval steps included</returns>
    Task<IEnumerable<Request>> GetPendingRequestsByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total count of all requests in the database.
    /// Used for generating sequential request numbers.
    /// </summary>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(Request request, CancellationToken cancellationToken = default);
    Task UpdateAsync(Request request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

