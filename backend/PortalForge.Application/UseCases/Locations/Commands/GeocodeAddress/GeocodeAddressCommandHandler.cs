using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;
using PortalForge.Application.Interfaces;

namespace PortalForge.Application.UseCases.Locations.Commands.GeocodeAddress;

public class GeocodeAddressCommandHandler : IRequestHandler<GeocodeAddressCommand, GeocodeAddressResult>
{
    private readonly IGeocodingService _geocodingService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUnifiedValidatorService _validatorService;
    private readonly ILogger<GeocodeAddressCommandHandler> _logger;

    public GeocodeAddressCommandHandler(
        IGeocodingService geocodingService,
        IUnitOfWork unitOfWork,
        IUnifiedValidatorService validatorService,
        ILogger<GeocodeAddressCommandHandler> logger)
    {
        _geocodingService = geocodingService;
        _unitOfWork = unitOfWork;
        _validatorService = validatorService;
        _logger = logger;
    }

    public async Task<GeocodeAddressResult> Handle(GeocodeAddressCommand request, CancellationToken cancellationToken)
    {
        // Validate command
        await _validatorService.ValidateAsync(request);

        _logger.LogInformation("Geocoding address: {Address}", request.Address);

        // Try geocoding service first (Nominatim API)
        var serviceResult = await _geocodingService.GeocodeAddressAsync(request.Address);
        if (serviceResult != null)
        {
            return new GeocodeAddressResult
            {
                Address = serviceResult.Address,
                Latitude = serviceResult.Latitude,
                Longitude = serviceResult.Longitude,
                Source = serviceResult.Source
            };
        }

        // Fallback: search in cached locations
        var cachedLocation = await _unitOfWork.CachedLocationRepository.SearchByAddressOrNameAsync(request.Address);
        if (cachedLocation != null)
        {
            return new GeocodeAddressResult
            {
                Address = cachedLocation.Address,
                Latitude = cachedLocation.Latitude,
                Longitude = cachedLocation.Longitude,
                Source = "CachedLocation"
            };
        }

        throw new NotFoundException("Location not found. Try with cached locations or add to cache.");
    }
}
