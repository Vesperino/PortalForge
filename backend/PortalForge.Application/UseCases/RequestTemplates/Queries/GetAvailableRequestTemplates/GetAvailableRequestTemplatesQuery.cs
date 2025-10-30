using MediatR;

namespace PortalForge.Application.UseCases.RequestTemplates.Queries.GetAvailableRequestTemplates;

public class GetAvailableRequestTemplatesQuery : IRequest<GetAvailableRequestTemplatesResult>
{
    public Guid UserId { get; set; }
}

