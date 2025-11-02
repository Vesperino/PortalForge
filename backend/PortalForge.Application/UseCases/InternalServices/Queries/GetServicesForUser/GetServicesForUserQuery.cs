using MediatR;
using PortalForge.Application.UseCases.InternalServices.DTOs;

namespace PortalForge.Application.UseCases.InternalServices.Queries.GetServicesForUser;

public class GetServicesForUserQuery : IRequest<List<InternalServiceDto>>
{
    public Guid UserId { get; set; }
}
