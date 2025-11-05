using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.Interfaces;

/// <summary>
/// Service for handling service requests with automatic routing and status tracking.
/// </summary>
public interface IServiceRequestHandler
{
    /// <summary>
    /// Process a service request by routing it to the appropriate service team.
    /// </summary>
    /// <param name="request">The service request to process</param>
    /// <returns>Result indicating success and routing information</returns>
    Task<ServiceRequestResult> ProcessServiceRequestAsync(Request request);

    /// <summary>
    /// Check if this handler can process the given request type.
    /// </summary>
    /// <param name="requestType">The type/category of request to validate</param>
    /// <returns>True if the request type can be handled</returns>
    Task<bool> CanHandleRequestTypeAsync(string requestType);

    /// <summary>
    /// Notify the appropriate service team about a new service request.
    /// </summary>
    /// <param name="request">The service request</param>
    /// <param name="serviceCategory">The service category for routing</param>
    Task NotifyServiceTeamAsync(Request request, string serviceCategory);

    /// <summary>
    /// Update the status of a service task and add notes.
    /// </summary>
    /// <param name="requestId">The ID of the service request</param>
    /// <param name="status">The new status</param>
    /// <param name="notes">Optional notes about the status update</param>
    Task UpdateServiceTaskStatusAsync(Guid requestId, ServiceTaskStatus status, string notes);
}

/// <summary>
/// Result of processing a service request.
/// </summary>
public class ServiceRequestResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? AssignedTeam { get; set; }
    public List<Guid> NotifiedUsers { get; set; } = new();
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}