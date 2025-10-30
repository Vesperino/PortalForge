using MediatR;

namespace PortalForge.Application.UseCases.RequestTemplates.Commands.UpdateRequestTemplate;

public class UpdateRequestTemplateCommand : IRequest<UpdateRequestTemplateResult>
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Category { get; set; }
    public int? EstimatedProcessingDays { get; set; }
    public bool? IsActive { get; set; }
}

