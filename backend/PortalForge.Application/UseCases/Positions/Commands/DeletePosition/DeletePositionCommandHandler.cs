using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;

namespace PortalForge.Application.UseCases.Positions.Commands.DeletePosition;

/// <summary>
/// Handler for deleting a position.
/// </summary>
public class DeletePositionCommandHandler : IRequestHandler<DeletePositionCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeletePositionCommandHandler> _logger;

    public DeletePositionCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<DeletePositionCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeletePositionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting position: {PositionId}", request.PositionId);

        var position = await _unitOfWork.PositionRepository.GetByIdAsync(request.PositionId);
        if (position == null)
        {
            _logger.LogWarning("Position not found: {PositionId}", request.PositionId);
            throw new NotFoundException($"Position with ID {request.PositionId} not found");
        }

        await _unitOfWork.PositionRepository.DeleteAsync(request.PositionId);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Position deleted successfully: {PositionId}", request.PositionId);
        return Unit.Value;
    }
}
