using MediatR;
using PortalForge.Application.UseCases.InternalServices.DTOs;

namespace PortalForge.Application.UseCases.InternalServices.Queries.GetServiceById;

public class GetServiceByIdQuery : IRequest<InternalServiceDto?>
{
    public Guid Id { get; set; }
}
