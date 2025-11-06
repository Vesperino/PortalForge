using PortalForge.Domain.Enums;

namespace PortalForge.Application.UseCases.RequestTemplates.DTOs;

public class RequestTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? DepartmentId { get; set; }
    public bool IsActive { get; set; }
    public bool RequiresApproval { get; set; }
    public int? EstimatedProcessingDays { get; set; }
    public int? PassingScore { get; set; }
    public Guid CreatedById { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<RequestTemplateFieldDto> Fields { get; set; } = new();
    public List<RequestApprovalStepTemplateDto> ApprovalStepTemplates { get; set; } = new();
    // QuizQuestions are now per approval step, not per template
}

public class RequestTemplateFieldDto
{
    public Guid Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string FieldType { get; set; } = string.Empty;
    public string? Placeholder { get; set; }
    public bool IsRequired { get; set; }
    public string? Options { get; set; }
    public int? MinValue { get; set; }
    public int? MaxValue { get; set; }
    public string? HelpText { get; set; }
    public int Order { get; set; }
}

public class RequestApprovalStepTemplateDto
{
    public Guid Id { get; set; }
    public int StepOrder { get; set; }
    public string ApproverType { get; set; } = "DirectSupervisor"; // DirectSupervisor, SpecificUser, SpecificDepartment, UserGroup, Submitter
    public Guid? SpecificUserId { get; set; } // For ApproverType = SpecificUser
    public Guid? SpecificDepartmentId { get; set; } // For ApproverType = SpecificDepartment
    public Guid? ApproverGroupId { get; set; } // For ApproverType = UserGroup
    public bool RequiresQuiz { get; set; }
    public int? PassingScore { get; set; } // Minimum score (0-100) required to pass this step's quiz
    public List<QuizQuestionDto> QuizQuestions { get; set; } = new(); // Quiz questions specific to this approval step
}

public class QuizQuestionDto
{
    public Guid Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Options { get; set; } = string.Empty;
    public int Order { get; set; }
}

