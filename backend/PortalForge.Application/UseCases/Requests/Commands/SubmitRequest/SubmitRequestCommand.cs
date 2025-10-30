using MediatR;

namespace PortalForge.Application.UseCases.Requests.Commands.SubmitRequest;

public class SubmitRequestCommand : IRequest<SubmitRequestResult>
{
    public Guid RequestTemplateId { get; set; }
    public Guid SubmittedById { get; set; }
    public string Priority { get; set; } = "Standard";
    public string FormData { get; set; } = string.Empty;
}

