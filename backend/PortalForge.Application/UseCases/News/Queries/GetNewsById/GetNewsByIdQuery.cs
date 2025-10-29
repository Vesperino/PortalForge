using MediatR;
using PortalForge.Application.UseCases.News.DTOs;

namespace PortalForge.Application.UseCases.News.Queries.GetNewsById;

public class GetNewsByIdQuery : IRequest<NewsDto>
{
    public int NewsId { get; set; }
}
