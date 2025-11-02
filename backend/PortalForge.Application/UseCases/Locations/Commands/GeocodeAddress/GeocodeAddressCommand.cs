using MediatR;

namespace PortalForge.Application.UseCases.Locations.Commands.GeocodeAddress;

public class GeocodeAddressCommand : IRequest<GeocodeAddressResult>
{
    public string Address { get; set; } = string.Empty;
}
