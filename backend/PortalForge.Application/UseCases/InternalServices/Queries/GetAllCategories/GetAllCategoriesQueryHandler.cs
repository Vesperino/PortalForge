using MediatR;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.InternalServices.DTOs;

namespace PortalForge.Application.UseCases.InternalServices.Queries.GetAllCategories;

public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, List<InternalServiceCategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllCategoriesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<InternalServiceCategoryDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _unitOfWork.InternalServiceCategoryRepository.GetAllAsync();

        return categories.Select(c => new InternalServiceCategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Icon = c.Icon,
            DisplayOrder = c.DisplayOrder,
            Services = c.Services.Select(s => new InternalServiceDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                Url = s.Url,
                Icon = s.Icon,
                IconType = s.IconType,
                DisplayOrder = s.DisplayOrder,
                IsActive = s.IsActive,
                IsGlobal = s.IsGlobal,
                IsPinned = s.IsPinned,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                DepartmentIds = s.ServiceDepartments.Select(sd => sd.DepartmentId).ToList()
            }).ToList()
        }).ToList();
    }
}
