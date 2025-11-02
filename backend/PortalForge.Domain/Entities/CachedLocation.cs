namespace PortalForge.Domain.Entities;

/// <summary>
/// Represents a cached location for offline geolocation support.
/// </summary>
public class CachedLocation
{
    public int Id { get; set; }
    
    /// <summary>
    /// Display name of the location (e.g., "Biuro Główne", "Sala Konferencyjna A")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Full address of the location
    /// </summary>
    public string Address { get; set; } = string.Empty;
    
    /// <summary>
    /// Latitude coordinate
    /// </summary>
    public decimal Latitude { get; set; }
    
    /// <summary>
    /// Longitude coordinate
    /// </summary>
    public decimal Longitude { get; set; }
    
    /// <summary>
    /// Type of location
    /// </summary>
    public LocationType Type { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public Guid CreatedBy { get; set; }
    public User? CreatedByUser { get; set; }
}

/// <summary>
/// Types of cached locations
/// </summary>
public enum LocationType
{
    Office,
    ConferenceRoom,
    Popular
}




