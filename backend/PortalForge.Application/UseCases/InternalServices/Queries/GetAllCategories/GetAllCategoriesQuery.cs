using MediatR;
using PortalForge.Application.UseCases.InternalServices.DTOs;

namespace PortalForge.Application.UseCases.InternalServices.Queries.GetAllCategories;

public class GetAllCategoriesQuery : IRequest<List<InternalServiceCategoryDto>>
{
}
