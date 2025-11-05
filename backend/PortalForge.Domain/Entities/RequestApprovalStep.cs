using PortalForge.Domain.Enums;

namespace PortalForge.Domain.Entities;

public class RequestApprovalStep
{
    public Guid Id { get; set; }
    
    public Guid RequestId { get; set; }
    public Request Request { get; set; } = null!;
    
    public int StepOrder { get; set; }
    
    public Guid ApproverId { get; set; }
    public User Approver { get; set; } = null!;
    
    /// <summary>
    /// The user currently assigned to approve this step (may be different from Approver due to delegation/escalation).
    /// </summary>
    public Guid? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }
    
    /// <summary>
    /// Reference to the step template that defines the approval rules.
    /// </summary>
    public Guid? StepTemplateId { get; set; }
    public RequestApprovalStepTemplate? StepTemplate { get; set; }
    
    public ApprovalStepStatus Status { get; set; } = ApprovalStepStatus.Pending;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public DateTime? EscalatedAt { get; set; }
    public string? Comment { get; set; }
    
    public bool RequiresQuiz { get; set; } = false;
    public int? QuizScore { get; set; }
    public bool? QuizPassed { get; set; }
    
    // Navigation properties
    public ICollection<QuizAnswer> QuizAnswers { get; set; } = new List<QuizAnswer>();
}

