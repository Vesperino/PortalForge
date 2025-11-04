using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Requests.Commands.AddRequestComment;

/// <summary>
/// Command to add a comment to a request.
/// Supports attachments (JSON array of file paths/URLs).
/// </summary>
public class AddRequestCommentCommand : IRequest<Guid>, ITransactionalRequest
{
    public Guid RequestId { get; set; }
    public Guid UserId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string? Attachments { get; set; }
}
