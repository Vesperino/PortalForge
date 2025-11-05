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

    /// <summary>
    /// Gets requests by service category for service team routing
    /// </summary>
    /// <param name="serviceCategory">Service category to filter by</param>
    /// <returns>List of requests in the specified service category</returns>
    Task<IEnumerable<Request>> GetByServiceCategoryAsync(string serviceCategory);

    /// <summary>
    /// Gets requests that can be used as templates (IsTemplate = true)
    /// </summary>
    /// <param name="userId">Optional user ID to filter by submitter</param>
    /// <returns>List of template requests</returns>
    Task<IEnumerable<Request>> GetTemplateRequestsAsync(Guid? userId = null);

    /// <summary>
    /// Gets requests cloned from a specific original request
    /// </summary>
    /// <param name="originalRequestId">ID of the original request</param>
    /// <returns>List of cloned requests</returns>
    Task<IEnumerable<Request>> GetClonedRequestsAsync(Guid originalRequestId);

    /// <summary>
    /// Gets requests with specific tags
    /// </summary>
    /// <param name="tags">List of tags to search for</param>
    /// <returns>List of requests containing any of the specified tags</returns>
    Task<IEnumerable<Request>> GetByTagsAsync(List<string> tags);

    /// <summary>
    /// Advanced search across request data with multiple criteria
    /// </summary>
    /// <param name="searchTerm">Search term to look for in request data</param>
    /// <param name="statusFilter">Optional status filter</param>
    /// <param name="templateIds">Optional template IDs filter</param>
    /// <param name="submittedAfter">Optional date filter for submission date</param>
    /// <param name="submittedBefore">Optional date filter for submission date</param>
    /// <returns>List of matching requests</returns>
    Task<IEnumerable<Request>> SearchRequestsAsync(
        string? searchTerm = null,
        List<RequestStatus>? statusFilter = null,
        List<Guid>? templateIds = null,
        DateTime? submittedAfter = null,
        DateTime? submittedBefore = null);

    Task<Guid> CreateAsync(Request request);
    Task UpdateAsync(Request request);
    Task DeleteAsync(Guid id);
}

