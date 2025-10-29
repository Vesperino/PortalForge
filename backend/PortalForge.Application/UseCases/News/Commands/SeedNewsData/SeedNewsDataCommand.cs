using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.News.Commands.SeedNewsData;

public class SeedNewsDataCommand : IRequest<int>, ITransactionalRequest
{
}
