namespace PortalForge.Application.UseCases.News.DTOs;

public class NewsDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int Views { get; set; }
    public string Category { get; set; } = string.Empty;
    public int? EventId { get; set; }

    // Event-specific fields
    public bool IsEvent { get; set; }
    public string? EventHashtag { get; set; }
    public DateTime? EventDateTime { get; set; }
    public string? EventLocation { get; set; }

    // Department visibility
    public int? DepartmentId { get; set; }
}
