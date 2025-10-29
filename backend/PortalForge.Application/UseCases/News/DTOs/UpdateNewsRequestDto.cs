namespace PortalForge.Application.UseCases.News.DTOs;

public class UpdateNewsRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string Category { get; set; } = string.Empty;
    public int? EventId { get; set; }
}
