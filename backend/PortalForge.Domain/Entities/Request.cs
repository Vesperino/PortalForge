using PortalForge.Domain.Enums;

namespace PortalForge.Domain.Entities;

public class Request
{
    public Guid Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    
    public Guid RequestTemplateId { get; set; }
    public RequestTemplate RequestTemplate { get; set; } = null!;
    
    public Guid SubmittedById { get; set; }
    public User SubmittedBy { get; set; } = null!;
    
    public DateTime SubmittedAt { get; set; }
    public RequestPriority Priority { get; set; } = RequestPriority.Standard;
    
    // JSON string with form field values
    public string FormData { get; set; } = string.Empty;

    /// <summary>
    /// Type of leave if this is a vacation/sick leave request. Null for other request types.
    /// </summary>
    public LeaveType? LeaveType { get; set; }

    /// <summary>
    /// JSON array of attachment file paths/URLs. Used for supporting documents.
    /// </summary>
    public string? Attachments { get; set; }

    public RequestStatus Status { get; set; } = RequestStatus.Draft;
    public DateTime? CompletedAt { get; set; }
    
    // New properties for enhanced functionality
    /// <summary>
    /// Service category for automatic routing to service teams
    /// </summary>
    public string? ServiceCategory { get; set; }
    
    /// <summary>
    /// Current status of service task if this is a service request
    /// </summary>
    public ServiceTaskStatus? ServiceStatus { get; set; }
    
    /// <summary>
    /// When the service task was completed
    /// </summary>
    public DateTime? ServiceCompletedAt { get; set; }
    
    /// <summary>
    /// Notes from service team about the task
    /// </summary>
    public string? ServiceNotes { get; set; }
    
    /// <summary>
    /// Indicates if this request can be used as a template for cloning
    /// </summary>
    public bool IsTemplate { get; set; } = false;
    
    /// <summary>
    /// Reference to the original request if this was cloned
    /// </summary>
    public Guid? ClonedFromId { get; set; }
    
    /// <summary>
    /// Reference to the original request entity if this was cloned
    /// </summary>
    public Request? ClonedFrom { get; set; }
    
    /// <summary>
    /// JSON array of tags for categorization and filtering
    /// </summary>
    public string? Tags { get; set; }
    
    // Navigation properties
    public ICollection<RequestApprovalStep> ApprovalSteps { get; set; } = new List<RequestApprovalStep>();
    public ICollection<Request> ClonedRequests { get; set; } = new List<Request>();
}

