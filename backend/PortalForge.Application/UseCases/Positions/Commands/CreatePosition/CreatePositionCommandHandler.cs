using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;

namespace PortalForge.Application.UseCases.Positions.Commands.CreatePosition;

/// <summary>
/// Handler for creating a new position.
/// </summary>
public class CreatePositionCommandHandler : IRequestHandler<CreatePositionCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreatePositionCommandHandler> _logger;

    public CreatePositionCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CreatePositionCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreatePositionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new position: {Name}", request.Name);

        var position = new Position
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        var positionId = await _unitOfWork.PositionRepository.CreateAsync(position);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Position created successfully with ID: {PositionId}", positionId);
        return positionId;
    }
}
