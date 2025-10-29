namespace PortalForge.Domain.Entities;

/// <summary>
/// Represents a news article in the portal.
/// </summary>
public class News
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }

    public Guid AuthorId { get; set; }
    public User? Author { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public int Views { get; set; } = 0;
    public NewsCategory Category { get; set; }

    public int? EventId { get; set; }
    public Event? Event { get; set; }

    // Event-specific fields
    public bool IsEvent { get; set; } = false;
    public string? EventHashtag { get; set; }
    public DateTime? EventDateTime { get; set; }
    public string? EventLocation { get; set; }

    // Department visibility
    public int? DepartmentId { get; set; }
}
