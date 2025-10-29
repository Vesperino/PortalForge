using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.News.Commands.DeleteNews;

public class DeleteNewsCommand : IRequest<Unit>, ITransactionalRequest
{
    public int NewsId { get; set; }
}
