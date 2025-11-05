using MediatR;

namespace PortalForge.Application.UseCases.Requests.Commands.CloneRequest;

public class CloneRequestCommand : IRequest<CloneRequestResult>
{
    public Guid OriginalRequestId { get; set; }
    public Guid ClonedById { get; set; }
    public string? ModifiedFormData { get; set; } // Optional - if user wants to modify data during cloning
    public bool CreateAsTemplate { get; set; } = false; // If true, creates a reusable template
}