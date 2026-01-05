using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.DTOs;

namespace PortalForge.Application.UseCases.Positions.Queries.GetAllPositions;

public class GetAllPositionsQueryHandler : IRequestHandler<GetAllPositionsQuery, GetAllPositionsResult>
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

    public async Task<GetAllPositionsResult> Handle(GetAllPositionsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting positions: SearchTerm={SearchTerm}, IsActive={IsActive}, Page={Page}",
            request.SearchTerm, request.IsActive, request.PageNumber);

        var (positions, totalCount) = await _unitOfWork.PositionRepository.GetFilteredAsync(
            request.SearchTerm,
            request.IsActive,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var positionDtos = positions.Select(p => new PositionDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        }).ToList();

        _logger.LogInformation("Found {Count} positions (total: {TotalCount})", positionDtos.Count, totalCount);

        return new GetAllPositionsResult
        {
            Positions = positionDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
