using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.DTOs;

namespace PortalForge.Application.UseCases.Positions.Queries.SearchPositions;

/// <summary>
/// Handler for searching positions by name.
/// </summary>
public class SearchPositionsQueryHandler : IRequestHandler<SearchPositionsQuery, List<PositionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SearchPositionsQueryHandler> _logger;

    public SearchPositionsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<SearchPositionsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<PositionDto>> Handle(SearchPositionsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Searching positions with term: {SearchTerm}", request.SearchTerm);

        if (string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            // Return top 10 active positions if no search term
            var activePositions = await _unitOfWork.PositionRepository.GetActiveAsync();
            return activePositions.Take(10).Select(p => new PositionDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).ToList();
        }

        var positions = await _unitOfWork.PositionRepository.SearchByNameAsync(request.SearchTerm);

        var result = positions.Select(p => new PositionDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        }).ToList();

        _logger.LogInformation("Found {Count} positions matching '{SearchTerm}'", result.Count, request.SearchTerm);
        return result;
    }
}
