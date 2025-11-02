using MediatR;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.InternalServices.DTOs;

namespace PortalForge.Application.UseCases.InternalServices.Queries.GetAllServices;

public class GetAllServicesQueryHandler : IRequestHandler<GetAllServicesQuery, List<InternalServiceDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllServicesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<InternalServiceDto>> Handle(GetAllServicesQuery request, CancellationToken cancellationToken)
    {
        var services = await _unitOfWork.InternalServiceRepository.GetAllAsync();

        return services.Select(s => new InternalServiceDto
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            Url = s.Url,
            Icon = s.Icon,
            IconType = s.IconType,
            CategoryId = s.CategoryId,
            CategoryName = s.Category?.Name,
            DisplayOrder = s.DisplayOrder,
            IsActive = s.IsActive,
            IsGlobal = s.IsGlobal,
            IsPinned = s.IsPinned,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt,
            DepartmentIds = s.ServiceDepartments.Select(sd => sd.DepartmentId).ToList()
        }).ToList();
    }
}
