namespace PortalForge.Application.UseCases.News.DTOs;

public class UpdateNewsRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string Category { get; set; } = string.Empty;
    public int? EventId { get; set; }

    // Event-specific fields
    public bool IsEvent { get; set; } = false;
    public string? EventHashtag { get; set; }
    public DateTime? EventDateTime { get; set; }
    public string? EventLocation { get; set; }
    public string? EventPlaceId { get; set; }
    public decimal? EventLatitude { get; set; }
    public decimal? EventLongitude { get; set; }

    // Department visibility
    public int? DepartmentId { get; set; }
    
    // Hashtags
    public List<string>? Hashtags { get; set; }
}
