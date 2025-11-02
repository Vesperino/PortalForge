using MediatR;
using PortalForge.Application.UseCases.InternalServices.DTOs;

namespace PortalForge.Application.UseCases.InternalServices.Queries.GetAllServices;

public class GetAllServicesQuery : IRequest<List<InternalServiceDto>>
{
}
