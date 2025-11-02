using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;

namespace PortalForge.Application.UseCases.InternalServices.Commands.UpdateService;

public class UpdateServiceCommandHandler : IRequestHandler<UpdateServiceCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUnifiedValidatorService _validatorService;
    private readonly ILogger<UpdateServiceCommandHandler> _logger;

    public UpdateServiceCommandHandler(
        IUnitOfWork unitOfWork,
        IUnifiedValidatorService validatorService,
        ILogger<UpdateServiceCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _validatorService = validatorService;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating internal service: {ServiceId}", request.Id);

        await _validatorService.ValidateAsync(request);

        var service = await _unitOfWork.InternalServiceRepository.GetByIdAsync(request.Id)
            ?? throw new NotFoundException($"Service with ID {request.Id} not found");

        service.Name = request.Name;
        service.Description = request.Description;
        service.Url = request.Url;
        service.Icon = request.Icon;
        service.IconType = request.IconType;
        service.CategoryId = request.CategoryId;
        service.DisplayOrder = request.DisplayOrder;
        service.IsActive = request.IsActive;
        service.IsGlobal = request.IsGlobal;
        service.IsPinned = request.IsPinned;
        service.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.InternalServiceRepository.UpdateAsync(service);

        if (!request.IsGlobal && request.DepartmentIds.Any())
        {
            await _unitOfWork.InternalServiceRepository.AssignToDepartmentsAsync(
                request.Id,
                request.DepartmentIds);
        }
        else if (request.IsGlobal)
        {
            await _unitOfWork.InternalServiceRepository.RemoveDepartmentAssignmentsAsync(request.Id);
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Internal service updated successfully: {ServiceId}", request.Id);

        return true;
    }
}
