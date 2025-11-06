namespace PortalForge.Domain.Entities;

public class QuizQuestion
{
    public Guid Id { get; set; }

    // Quiz is now per approval step, not per template
    public Guid RequestApprovalStepTemplateId { get; set; }
    public RequestApprovalStepTemplate RequestApprovalStepTemplate { get; set; } = null!;

    public string Question { get; set; } = string.Empty;

    // JSON array: [{value: "a", label: "Answer A", isCorrect: true}, ...]
    public string Options { get; set; } = string.Empty;

    public int Order { get; set; }

    // Navigation properties
    public ICollection<QuizAnswer> QuizAnswers { get; set; } = new List<QuizAnswer>();
}

