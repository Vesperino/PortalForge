using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;

namespace PortalForge.Application.UseCases.Locations.Commands.DeleteCachedLocation;

public class DeleteCachedLocationCommandHandler : IRequestHandler<DeleteCachedLocationCommand, DeleteCachedLocationResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteCachedLocationCommandHandler> _logger;

    public DeleteCachedLocationCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<DeleteCachedLocationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<DeleteCachedLocationResult> Handle(DeleteCachedLocationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting cached location with ID: {Id}", request.Id);

        // Check if location exists
        var location = await _unitOfWork.CachedLocationRepository.GetByIdAsync(request.Id);
        if (location == null)
        {
            throw new NotFoundException($"Cached location with ID {request.Id} not found");
        }

        // Delete location
        await _unitOfWork.CachedLocationRepository.DeleteAsync(request.Id);

        _logger.LogInformation("Cached location deleted successfully: {Id}", request.Id);

        return new DeleteCachedLocationResult();
    }
}
