using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;

namespace PortalForge.Application.UseCases.Positions.Commands.UpdatePosition;

/// <summary>
/// Handler for updating a position.
/// </summary>
public class UpdatePositionCommandHandler : IRequestHandler<UpdatePositionCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdatePositionCommandHandler> _logger;

    public UpdatePositionCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdatePositionCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdatePositionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating position: {PositionId}", request.PositionId);

        var position = await _unitOfWork.PositionRepository.GetByIdAsync(request.PositionId);
        if (position == null)
        {
            _logger.LogWarning("Position not found: {PositionId}", request.PositionId);
            throw new NotFoundException($"Position with ID {request.PositionId} not found");
        }

        position.Name = request.Name;
        position.Description = request.Description;
        position.IsActive = request.IsActive;
        position.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.PositionRepository.UpdateAsync(position);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Position updated successfully: {PositionId}", request.PositionId);
        return Unit.Value;
    }
}
