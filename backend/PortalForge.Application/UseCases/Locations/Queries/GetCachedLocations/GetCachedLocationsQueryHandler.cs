using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Locations.Queries.GetCachedLocations;

public class GetCachedLocationsQueryHandler : IRequestHandler<GetCachedLocationsQuery, GetCachedLocationsResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetCachedLocationsQueryHandler> _logger;

    public GetCachedLocationsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetCachedLocationsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<GetCachedLocationsResult> Handle(GetCachedLocationsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all cached locations");

        var locations = await _unitOfWork.CachedLocationRepository.GetAllAsync();

        return new GetCachedLocationsResult
        {
            Locations = locations.Select(l => new CachedLocationDto
            {
                Id = l.Id,
                Name = l.Name,
                Address = l.Address,
                Latitude = l.Latitude,
                Longitude = l.Longitude,
                Type = l.Type.ToString()
            }).ToList()
        };
    }
}
