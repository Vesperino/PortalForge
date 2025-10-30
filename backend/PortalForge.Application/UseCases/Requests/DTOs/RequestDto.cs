namespace PortalForge.Application.UseCases.Requests.DTOs;

public class RequestDto
{
    public Guid Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public Guid RequestTemplateId { get; set; }
    public string RequestTemplateName { get; set; } = string.Empty;
    public string RequestTemplateIcon { get; set; } = string.Empty;
    public Guid SubmittedById { get; set; }
    public string SubmittedByName { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public string Priority { get; set; } = string.Empty;
    public string FormData { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
    
    public List<RequestApprovalStepDto> ApprovalSteps { get; set; } = new();
}

public class RequestApprovalStepDto
{
    public Guid Id { get; set; }
    public int StepOrder { get; set; }
    public Guid ApproverId { get; set; }
    public string ApproverName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public string? Comment { get; set; }
    public bool RequiresQuiz { get; set; }
    public int? QuizScore { get; set; }
    public bool? QuizPassed { get; set; }
}

