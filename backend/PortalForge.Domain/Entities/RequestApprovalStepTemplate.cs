using PortalForge.Domain.Enums;

namespace PortalForge.Domain.Entities;

public class RequestApprovalStepTemplate
{
    public Guid Id { get; set; }
    public Guid RequestTemplateId { get; set; }
    public RequestTemplate RequestTemplate { get; set; } = null!;
    
    public int StepOrder { get; set; }
    public DepartmentRole ApproverRole { get; set; }
    public bool RequiresQuiz { get; set; } = false;
    
    public DateTime CreatedAt { get; set; }
}

