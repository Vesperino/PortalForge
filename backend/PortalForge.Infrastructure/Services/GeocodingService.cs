using Microsoft.Extensions.Logging;
using PortalForge.Application.Interfaces;
using System.Text.Json;

namespace PortalForge.Infrastructure.Services;

public class GeocodingService : IGeocodingService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GeocodingService> _logger;

    public GeocodingService(
        IHttpClientFactory httpClientFactory,
        ILogger<GeocodingService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<GeocodeServiceResult?> GeocodeAddressAsync(string address)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "PortalForge/1.0");

            var nominatimUrl = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json&limit=1";

            var response = await httpClient.GetAsync(nominatimUrl);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var results = JsonSerializer.Deserialize<List<NominatimResult>>(content);

                if (results != null && results.Count > 0)
                {
                    var result = results[0];
                    return new GeocodeServiceResult
                    {
                        Address = result.display_name ?? address,
                        Latitude = decimal.Parse(result.lat ?? "0"),
                        Longitude = decimal.Parse(result.lon ?? "0"),
                        Source = "Nominatim"
                    };
                }
            }

            _logger.LogWarning("Nominatim API returned no results for address: {Address}", address);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Nominatim API failed for address: {Address}", address);
            return null;
        }
    }

    private class NominatimResult
    {
        public string? display_name { get; set; }
        public string? lat { get; set; }
        public string? lon { get; set; }
    }
}
