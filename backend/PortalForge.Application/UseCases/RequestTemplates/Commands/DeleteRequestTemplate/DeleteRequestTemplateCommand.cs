using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.RequestTemplates.Commands.DeleteRequestTemplate;

public class DeleteRequestTemplateCommand : IRequest<DeleteRequestTemplateResult>, ITransactionalRequest
{
    public Guid Id { get; set; }
    public Guid DeletedBy { get; set; }
}

