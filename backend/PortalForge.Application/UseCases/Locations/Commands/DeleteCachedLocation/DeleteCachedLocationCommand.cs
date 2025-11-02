using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Locations.Commands.DeleteCachedLocation;

public class DeleteCachedLocationCommand : IRequest<DeleteCachedLocationResult>, ITransactionalRequest
{
    public int Id { get; set; }
}
