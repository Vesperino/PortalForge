using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Positions.Commands.CreatePosition;

/// <summary>
/// Command to create a new position.
/// </summary>
public class CreatePositionCommand : IRequest<Guid>, ITransactionalRequest
{
    /// <summary>
    /// Position name/title.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the position.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether the position is active. Default: true.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
