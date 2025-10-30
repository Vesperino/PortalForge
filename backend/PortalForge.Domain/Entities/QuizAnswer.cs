namespace PortalForge.Domain.Entities;

public class QuizAnswer
{
    public Guid Id { get; set; }
    
    public Guid RequestApprovalStepId { get; set; }
    public RequestApprovalStep RequestApprovalStep { get; set; } = null!;
    
    public Guid QuizQuestionId { get; set; }
    public QuizQuestion QuizQuestion { get; set; } = null!;
    
    public string SelectedAnswer { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    
    public DateTime AnsweredAt { get; set; }
}

