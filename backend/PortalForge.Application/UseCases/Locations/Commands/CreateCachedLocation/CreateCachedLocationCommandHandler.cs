using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;

namespace PortalForge.Application.UseCases.Locations.Commands.CreateCachedLocation;

public class CreateCachedLocationCommandHandler : IRequestHandler<CreateCachedLocationCommand, CreateCachedLocationResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUnifiedValidatorService _validatorService;
    private readonly ILogger<CreateCachedLocationCommandHandler> _logger;

    public CreateCachedLocationCommandHandler(
        IUnitOfWork unitOfWork,
        IUnifiedValidatorService validatorService,
        ILogger<CreateCachedLocationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _validatorService = validatorService;
        _logger = logger;
    }

    public async Task<CreateCachedLocationResult> Handle(CreateCachedLocationCommand request, CancellationToken cancellationToken)
    {
        // Validate command
        await _validatorService.ValidateAsync(request);

        _logger.LogInformation("Creating cached location: {Name} at {Address}", request.Name, request.Address);

        // Create entity
        var location = new CachedLocation
        {
            Name = request.Name,
            Address = request.Address,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Type = Enum.Parse<LocationType>(request.Type, true),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy
        };

        // Save to database
        var locationId = await _unitOfWork.CachedLocationRepository.CreateAsync(location);

        _logger.LogInformation("Cached location created successfully with ID: {Id}", locationId);

        return new CreateCachedLocationResult
        {
            Id = locationId,
            Name = location.Name,
            Address = location.Address,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            Type = location.Type.ToString()
        };
    }
}
