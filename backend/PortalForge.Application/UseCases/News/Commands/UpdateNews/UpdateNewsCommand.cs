using MediatR;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;

namespace PortalForge.Application.UseCases.News.Commands.UpdateNews;

public class UpdateNewsCommand : IRequest<Unit>, ITransactionalRequest
{
    public int NewsId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public NewsCategory Category { get; set; }
    public int? EventId { get; set; }

    // Event-specific fields
    public bool IsEvent { get; set; } = false;
    public string? EventHashtag { get; set; }
    public DateTime? EventDateTime { get; set; }
    public string? EventLocation { get; set; }

    // Department visibility
    public int? DepartmentId { get; set; }
}
