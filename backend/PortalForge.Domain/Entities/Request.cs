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
    
    public RequestStatus Status { get; set; } = RequestStatus.Draft;
    public DateTime? CompletedAt { get; set; }
    
    // Navigation properties
    public ICollection<RequestApprovalStep> ApprovalSteps { get; set; } = new List<RequestApprovalStep>();
}

