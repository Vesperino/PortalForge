namespace PortalForge.Application.UseCases.Locations.Queries.GetCachedLocations;

public class CachedLocationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string Type { get; set; } = string.Empty;
}
