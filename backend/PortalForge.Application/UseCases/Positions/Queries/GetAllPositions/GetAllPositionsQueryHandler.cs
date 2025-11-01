using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.DTOs;

namespace PortalForge.Application.UseCases.Positions.Queries.GetAllPositions;

/// <summary>
/// Handler for getting all positions.
/// </summary>
public class GetAllPositionsQueryHandler : IRequestHandler<GetAllPositionsQuery, List<PositionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAllPositionsQueryHandler> _logger;

    public GetAllPositionsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAllPositionsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<PositionDto>> Handle(GetAllPositionsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all positions. ActiveOnly: {ActiveOnly}", request.ActiveOnly);

        var positions = request.ActiveOnly
            ? await _unitOfWork.PositionRepository.GetActiveAsync()
            : await _unitOfWork.PositionRepository.GetAllAsync();

        var result = positions.Select(p => new PositionDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        }).ToList();

        _logger.LogInformation("Found {Count} positions", result.Count);
        return result;
    }
}
