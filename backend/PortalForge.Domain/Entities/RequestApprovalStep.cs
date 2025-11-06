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
    
    public ApprovalStepStatus Status { get; set; } = ApprovalStepStatus.Pending;
    
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public string? Comment { get; set; }
    
    public bool RequiresQuiz { get; set; } = false;
    public int? PassingScore { get; set; }
    public int? QuizScore { get; set; }
    public bool? QuizPassed { get; set; }

    // Link to template step to access quiz questions
    public Guid? RequestApprovalStepTemplateId { get; set; }
    public RequestApprovalStepTemplate? ApprovalStepTemplate { get; set; }

    // Navigation properties
    public ICollection<QuizAnswer> QuizAnswers { get; set; } = new List<QuizAnswer>();
}

