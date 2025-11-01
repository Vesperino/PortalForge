using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Positions.Commands.UpdatePosition;

/// <summary>
/// Command to update an existing position.
/// </summary>
public class UpdatePositionCommand : IRequest<Unit>, ITransactionalRequest
{
    /// <summary>
    /// Position ID to update.
    /// </summary>
    public Guid PositionId { get; set; }

    /// <summary>
    /// New position name/title.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// New description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether the position is active.
    /// </summary>
    public bool IsActive { get; set; }
}
