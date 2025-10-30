using MediatR;

namespace PortalForge.Application.UseCases.RequestTemplates.Queries.GetRequestTemplateById;

public class GetRequestTemplateByIdQuery : IRequest<GetRequestTemplateByIdResult>
{
    public Guid Id { get; set; }
}

