using MediatR;

namespace PortalForge.Application.UseCases.Positions.Queries.GetAllPositions;

public class GetAllPositionsQuery : IRequest<GetAllPositionsResult>
{
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
