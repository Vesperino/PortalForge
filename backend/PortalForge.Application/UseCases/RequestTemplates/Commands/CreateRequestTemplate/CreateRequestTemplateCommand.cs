using MediatR;
using PortalForge.Application.UseCases.RequestTemplates.DTOs;

namespace PortalForge.Application.UseCases.RequestTemplates.Commands.CreateRequestTemplate;

public class CreateRequestTemplateCommand : IRequest<CreateRequestTemplateResult>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? DepartmentId { get; set; }
    public bool RequiresApproval { get; set; } = true;
    public int? EstimatedProcessingDays { get; set; }
    public int? PassingScore { get; set; }
    public Guid CreatedById { get; set; }
    
    public List<RequestTemplateFieldDto> Fields { get; set; } = new();
    public List<RequestApprovalStepTemplateDto> ApprovalStepTemplates { get; set; } = new();
    public List<QuizQuestionDto> QuizQuestions { get; set; } = new();
}

