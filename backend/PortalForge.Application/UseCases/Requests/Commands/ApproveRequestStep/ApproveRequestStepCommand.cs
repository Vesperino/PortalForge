using MediatR;

namespace PortalForge.Application.UseCases.Requests.Commands.ApproveRequestStep;

public class ApproveRequestStepCommand : IRequest<ApproveRequestStepResult>
{
    public Guid RequestId { get; set; }
    public Guid StepId { get; set; }
    public Guid ApproverId { get; set; }
    public string? Comment { get; set; }
}

