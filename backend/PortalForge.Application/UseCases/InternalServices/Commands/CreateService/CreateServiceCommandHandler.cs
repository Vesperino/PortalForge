using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;

namespace PortalForge.Application.UseCases.InternalServices.Commands.CreateService;

public class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUnifiedValidatorService _validatorService;
    private readonly ILogger<CreateServiceCommandHandler> _logger;

    public CreateServiceCommandHandler(
        IUnitOfWork unitOfWork,
        IUnifiedValidatorService validatorService,
        ILogger<CreateServiceCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _validatorService = validatorService;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating internal service: {Name}", request.Name);

        await _validatorService.ValidateAsync(request);

        var service = new InternalService
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Url = request.Url,
            Icon = request.Icon,
            IconType = request.IconType,
            CategoryId = request.CategoryId,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            IsGlobal = request.IsGlobal,
            IsPinned = request.IsPinned,
            CreatedById = request.CreatedById,
            CreatedAt = DateTime.UtcNow
        };

        var serviceId = await _unitOfWork.InternalServiceRepository.CreateAsync(service);

        if (!request.IsGlobal && request.DepartmentIds.Any())
        {
            await _unitOfWork.InternalServiceRepository.AssignToDepartmentsAsync(
                serviceId,
                request.DepartmentIds);
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Internal service created successfully with ID: {ServiceId}", serviceId);

        return serviceId;
    }
}
