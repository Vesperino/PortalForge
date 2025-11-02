namespace PortalForge.Api.DTOs.Requests.Locations;

public class CreateCachedLocationRequest
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string Type { get; set; } = "Popular";
}
