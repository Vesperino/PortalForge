using MediatR;
using PortalForge.Application.UseCases.News.DTOs;

namespace PortalForge.Application.UseCases.News.Queries.GetAllNews;

public class GetAllNewsQuery : IRequest<PaginatedNewsResponse>
{
    public string? Category { get; set; }
    public int? DepartmentId { get; set; }
    public bool? IsEvent { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
