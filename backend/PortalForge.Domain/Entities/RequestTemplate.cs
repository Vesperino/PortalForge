namespace PortalForge.Domain.Entities;

public class RequestTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    
    // Null = visible for all departments
    public string? DepartmentId { get; set; }
    
    public bool IsActive { get; set; } = true;
    public bool RequiresApproval { get; set; } = true;
    public int? EstimatedProcessingDays { get; set; }
    
    // Quiz passing score (0-100)
    public int? PassingScore { get; set; }
    
    public Guid CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<RequestTemplateField> Fields { get; set; } = new List<RequestTemplateField>();
    public ICollection<RequestApprovalStepTemplate> ApprovalStepTemplates { get; set; } = new List<RequestApprovalStepTemplate>();
    public ICollection<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();
    public ICollection<Request> Requests { get; set; } = new List<Request>();
}

