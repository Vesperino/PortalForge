using MediatR;
using PortalForge.Application.UseCases.News.DTOs;

namespace PortalForge.Application.UseCases.News.Queries.GetAllNews;

public class GetAllNewsQuery : IRequest<IEnumerable<NewsDto>>
{
    public string? Category { get; set; }
}
