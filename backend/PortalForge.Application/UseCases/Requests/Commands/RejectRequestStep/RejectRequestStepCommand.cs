using MediatR;

namespace PortalForge.Application.UseCases.Requests.Commands.RejectRequestStep;

public class RejectRequestStepCommand : IRequest<RejectRequestStepResult>
{
    public Guid RequestId { get; set; }
    public Guid StepId { get; set; }
    public Guid ApproverId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

