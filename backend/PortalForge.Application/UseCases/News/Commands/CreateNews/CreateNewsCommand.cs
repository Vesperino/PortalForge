using MediatR;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;

namespace PortalForge.Application.UseCases.News.Commands.CreateNews;

public class CreateNewsCommand : IRequest<int>, ITransactionalRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public Guid AuthorId { get; set; }
    public NewsCategory Category { get; set; }
    public int? EventId { get; set; }

    // Event-specific fields
    public bool IsEvent { get; set; } = false;
    public string? EventHashtag { get; set; }
    public DateTime? EventDateTime { get; set; }
    public string? EventLocation { get; set; }
    public string? EventPlaceId { get; set; }

    // Department visibility
    public int? DepartmentId { get; set; }
}
