using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalForge.Domain.Entities;
using PortalForge.Infrastructure.Persistence;
using System.Text.Json;

namespace PortalForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LocationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LocationsController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public LocationsController(
        ApplicationDbContext context,
        ILogger<LocationsController> logger,
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Get all cached locations
    /// </summary>
    [HttpGet("cached")]
    public async Task<ActionResult<List<CachedLocationDto>>> GetCachedLocations()
    {
        var locations = await _context.CachedLocations
            .OrderBy(l => l.Type)
            .ThenBy(l => l.Name)
            .Select(l => new CachedLocationDto
            {
                Id = l.Id,
                Name = l.Name,
                Address = l.Address,
                Latitude = l.Latitude,
                Longitude = l.Longitude,
                Type = l.Type.ToString()
            })
            .ToListAsync();

        return Ok(locations);
    }

    /// <summary>
    /// Geocode an address using OpenStreetMap Nominatim API
    /// </summary>
    [HttpPost("geocode")]
    public async Task<ActionResult<GeocodeResult>> GeocodeAddress([FromBody] GeocodeRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Address))
            {
                return BadRequest(new { message = "Address is required" });
            }

            _logger.LogInformation("Geocoding address: {Address}", request.Address);

            // Try Nominatim API first
            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "PortalForge/1.0");

            var nominatimUrl = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(request.Address)}&format=json&limit=1";

            try
            {
                var response = await httpClient.GetAsync(nominatimUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var results = JsonSerializer.Deserialize<List<NominatimResult>>(content);

                    if (results != null && results.Count > 0)
                    {
                        var result = results[0];
                        return Ok(new GeocodeResult
                        {
                            Address = result.display_name ?? request.Address,
                            Latitude = decimal.Parse(result.lat ?? "0"),
                            Longitude = decimal.Parse(result.lon ?? "0"),
                            Source = "Nominatim"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Nominatim API failed, falling back to cached locations");
            }

            // Fallback: search in cached locations
            var cachedLocation = await _context.CachedLocations
                .Where(l => l.Address.Contains(request.Address) || l.Name.Contains(request.Address))
                .FirstOrDefaultAsync();

            if (cachedLocation != null)
            {
                return Ok(new GeocodeResult
                {
                    Address = cachedLocation.Address,
                    Latitude = cachedLocation.Latitude,
                    Longitude = cachedLocation.Longitude,
                    Source = "CachedLocation"
                });
            }

            return NotFound(new { message = "Location not found. Try with cached locations or add to cache." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error geocoding address");
            return StatusCode(500, new { message = "Error geocoding address", error = ex.Message });
        }
    }

    /// <summary>
    /// Add a cached location (Admin only)
    /// </summary>
    [HttpPost("admin/cached")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CachedLocationDto>> AddCachedLocation([FromBody] CreateCachedLocationRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var location = new CachedLocation
            {
                Name = request.Name,
                Address = request.Address,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Type = Enum.Parse<LocationType>(request.Type),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Parse(userIdClaim)
            };

            _context.CachedLocations.Add(location);
            await _context.SaveChangesAsync();

            return Ok(new CachedLocationDto
            {
                Id = location.Id,
                Name = location.Name,
                Address = location.Address,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                Type = location.Type.ToString()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding cached location");
            return StatusCode(500, new { message = "Error adding location", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a cached location (Admin only)
    /// </summary>
    [HttpDelete("admin/cached/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteCachedLocation(int id)
    {
        try
        {
            var location = await _context.CachedLocations.FindAsync(id);
            if (location == null)
            {
                return NotFound(new { message = "Location not found" });
            }

            _context.CachedLocations.Remove(location);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Location deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting cached location");
            return StatusCode(500, new { message = "Error deleting location", error = ex.Message });
        }
    }
}

public class CachedLocationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string Type { get; set; } = string.Empty;
}

public class GeocodeRequest
{
    public string Address { get; set; } = string.Empty;
}

public class GeocodeResult
{
    public string Address { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string Source { get; set; } = string.Empty;
}

public class CreateCachedLocationRequest
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string Type { get; set; } = "Popular";
}

public class NominatimResult
{
    public string? display_name { get; set; }
    public string? lat { get; set; }
    public string? lon { get; set; }
}


