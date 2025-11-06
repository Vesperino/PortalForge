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
    public int? PassingScore { get; set; }
    public List<QuizQuestionDto> QuizQuestions { get; set; } = new();
    public List<QuizAnswerDto> QuizAnswers { get; set; } = new();
}

public class QuizQuestionDto
{
    public Guid Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Options { get; set; } = string.Empty;
    public int Order { get; set; }
}

public class QuizAnswerDto
{
    public Guid QuestionId { get; set; }
    public string SelectedAnswer { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public DateTime AnsweredAt { get; set; }
}

/// <summary>
/// Detailed request DTO including comments and edit history
/// </summary>
public class RequestDetailDto
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
    public string? LeaveType { get; set; }
    public List<string> Attachments { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
    public List<ApprovalStepDto> ApprovalSteps { get; set; } = new();
    public List<RequestCommentDto> Comments { get; set; } = new();
    public List<RequestEditHistoryDto> EditHistory { get; set; } = new();
}

/// <summary>
/// DTO for request comment
/// </summary>
public class RequestCommentDto
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public List<string> Attachments { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for request edit history entry
/// </summary>
public class RequestEditHistoryDto
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public Guid EditedByUserId { get; set; }
    public string EditedByUserName { get; set; } = string.Empty;
    public DateTime EditedAt { get; set; }
    public string OldFormData { get; set; } = string.Empty;
    public string NewFormData { get; set; } = string.Empty;
    public string? ChangeReason { get; set; }
}

/// <summary>
/// DTO for approval step used in detail view
/// </summary>
public class ApprovalStepDto
{
    public Guid Id { get; set; }
    public int StepOrder { get; set; }
    public Guid ApproverId { get; set; }
    public string ApproverName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public DateTime? FinishedAt { get; set; }
    public bool RequiresQuiz { get; set; }
    public int? QuizScore { get; set; }
    public bool? QuizPassed { get; set; }
    public int? PassingScore { get; set; }
    public List<QuizQuestionDto> QuizQuestions { get; set; } = new();
}

