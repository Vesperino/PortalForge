using MediatR;
using PortalForge.Application.UseCases.RequestTemplates.DTOs;

namespace PortalForge.Application.UseCases.RequestTemplates.Commands.UpdateRequestTemplate;

public class UpdateRequestTemplateCommand : IRequest<UpdateRequestTemplateResult>
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Category { get; set; }
    public string? DepartmentId { get; set; }
    public bool? RequiresApproval { get; set; }
    public int? EstimatedProcessingDays { get; set; }
    public int? PassingScore { get; set; }
    public bool? IsActive { get; set; }

    public List<RequestTemplateFieldDto>? Fields { get; set; }
    public List<RequestApprovalStepTemplateDto>? ApprovalStepTemplates { get; set; }
    // QuizQuestions are now nested within ApprovalStepTemplates
}

