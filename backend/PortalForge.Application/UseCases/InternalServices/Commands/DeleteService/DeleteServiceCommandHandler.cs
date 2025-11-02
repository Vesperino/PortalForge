using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;

namespace PortalForge.Application.UseCases.InternalServices.Commands.DeleteService;

public class DeleteServiceCommandHandler : IRequestHandler<DeleteServiceCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteServiceCommandHandler> _logger;

    public DeleteServiceCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<DeleteServiceCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteServiceCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting internal service: {ServiceId}", request.Id);

        var service = await _unitOfWork.InternalServiceRepository.GetByIdAsync(request.Id)
            ?? throw new NotFoundException($"Service with ID {request.Id} not found");

        await _unitOfWork.InternalServiceRepository.DeleteAsync(request.Id);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Internal service deleted successfully: {ServiceId}", request.Id);

        return true;
    }
}
