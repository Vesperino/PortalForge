namespace PortalForge.Application.UseCases.Locations.Commands.GeocodeAddress;

public class GeocodeAddressResult
{
    public string Address { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string Source { get; set; } = string.Empty;
}
