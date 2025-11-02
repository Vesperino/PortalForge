using MediatR;
using PortalForge.Application.DTOs;

namespace PortalForge.Application.UseCases.Positions.Queries.SearchPositions;

/// <summary>
/// Query to search positions by name for autocomplete.
/// </summary>
public class SearchPositionsQuery : IRequest<List<PositionDto>>
{
    /// <summary>
    /// Search term to filter positions by name.
    /// </summary>
    public string SearchTerm { get; set; } = string.Empty;
}
