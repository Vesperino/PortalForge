using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Locations.Commands.CreateCachedLocation;

public class CreateCachedLocationCommand : IRequest<CreateCachedLocationResult>, ITransactionalRequest
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string Type { get; set; } = "Popular";
    public Guid CreatedBy { get; set; }
}
