namespace PortalForge.Domain.Entities;

public class QuizQuestion
{
    public Guid Id { get; set; }
    
    public Guid RequestTemplateId { get; set; }
    public RequestTemplate RequestTemplate { get; set; } = null!;
    
    public string Question { get; set; } = string.Empty;
    
    // JSON array: [{value: "a", label: "Answer A", isCorrect: true}, ...]
    public string Options { get; set; } = string.Empty;
    
    public int Order { get; set; }
    
    // Navigation properties
    public ICollection<QuizAnswer> QuizAnswers { get; set; } = new List<QuizAnswer>();
}

