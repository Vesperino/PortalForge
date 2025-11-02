namespace PortalForge.Application.Interfaces;

public interface IGeocodingService
{
    Task<GeocodeServiceResult?> GeocodeAddressAsync(string address);
}

public class GeocodeServiceResult
{
    public string Address { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string Source { get; set; } = string.Empty;
}
