using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.InternalServices.Commands.DeleteService;

public class DeleteServiceCommand : IRequest<bool>, ITransactionalRequest
{
    public Guid Id { get; set; }
}
