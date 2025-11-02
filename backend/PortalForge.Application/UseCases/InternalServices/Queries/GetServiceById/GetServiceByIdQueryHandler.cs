using MediatR;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.InternalServices.DTOs;

namespace PortalForge.Application.UseCases.InternalServices.Queries.GetServiceById;

public class GetServiceByIdQueryHandler : IRequestHandler<GetServiceByIdQuery, InternalServiceDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetServiceByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<InternalServiceDto?> Handle(GetServiceByIdQuery request, CancellationToken cancellationToken)
    {
        var service = await _unitOfWork.InternalServiceRepository.GetByIdAsync(request.Id);

        if (service == null)
            return null;

        return new InternalServiceDto
        {
            Id = service.Id,
            Name = service.Name,
            Description = service.Description,
            Url = service.Url,
            Icon = service.Icon,
            IconType = service.IconType,
            CategoryId = service.CategoryId,
            CategoryName = service.Category?.Name,
            DisplayOrder = service.DisplayOrder,
            IsActive = service.IsActive,
            IsGlobal = service.IsGlobal,
            IsPinned = service.IsPinned,
            CreatedAt = service.CreatedAt,
            UpdatedAt = service.UpdatedAt,
            DepartmentIds = service.ServiceDepartments.Select(sd => sd.DepartmentId).ToList()
        };
    }
}
