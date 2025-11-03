using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Requests.Commands.EditRequest;

/// <summary>
/// Command to edit an existing request.
/// Only submitter can edit, and only Draft or InReview requests.
/// All edits are tracked in RequestEditHistory for audit trail.
/// </summary>
public class EditRequestCommand : IRequest<Unit>, ITransactionalRequest
{
    public Guid RequestId { get; set; }
    public Guid EditedByUserId { get; set; }
    public string NewFormData { get; set; } = string.Empty;
    public string? ChangeReason { get; set; }
}
