using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Positions.Commands.DeletePosition;

/// <summary>
/// Command to delete a position.
/// </summary>
public class DeletePositionCommand : IRequest<Unit>, ITransactionalRequest
{
    /// <summary>
    /// Position ID to delete.
    /// </summary>
    public Guid PositionId { get; set; }
}
