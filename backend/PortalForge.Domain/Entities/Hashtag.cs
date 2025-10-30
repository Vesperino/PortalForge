namespace PortalForge.Domain.Entities;

/// <summary>
/// Represents a hashtag that can be associated with news articles.
/// </summary>
public class Hashtag
{
    public int Id { get; set; }
    
    /// <summary>
    /// Original hashtag name (e.g., "#Wigilia")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Normalized lowercase name for searching (e.g., "#wigilia")
    /// </summary>
    public string NormalizedName { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation property
    public ICollection<News> News { get; set; } = new List<News>();
}

