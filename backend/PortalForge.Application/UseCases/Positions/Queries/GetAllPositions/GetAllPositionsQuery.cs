using MediatR;
using PortalForge.Application.DTOs;

namespace PortalForge.Application.UseCases.Positions.Queries.GetAllPositions;

/// <summary>
/// Query to get all positions.
/// </summary>
public class GetAllPositionsQuery : IRequest<List<PositionDto>>
{
    /// <summary>
    /// Whether to include only active positions. Default: true.
    /// </summary>
    public bool ActiveOnly { get; set; } = true;
}
