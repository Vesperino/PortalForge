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
    public bool RequiresSubstituteSelection { get; set; } = false;
    public int? EstimatedProcessingDays { get; set; }

    /// <summary>
    /// Whether this template allows file attachments to be uploaded.
    /// </summary>
    public bool AllowsAttachments { get; set; } = false;

    /// <summary>
    /// Maximum number of days in the past that this request can be submitted for.
    /// Null = no retrospective submission allowed. E.g., 14 for sick leave (can report up to 14 days back).
    /// </summary>
    public int? MaxRetrospectiveDays { get; set; }

    /// <summary>
    /// Indicates this is a vacation request template. Auto-creates VacationSchedule when approved.
    /// </summary>
    public bool IsVacationRequest { get; set; } = false;

    /// <summary>
    /// Indicates this is a sick leave (L4) request template. Auto-approved and creates SickLeave record.
    /// </summary>
    public bool IsSickLeaveRequest { get; set; } = false;

    /// <summary>
    /// Service category for automatic routing to service teams (e.g., "IT", "HR", "Facilities").
    /// Null for non-service request templates.
    /// </summary>
    public string? ServiceCategory { get; set; }

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

