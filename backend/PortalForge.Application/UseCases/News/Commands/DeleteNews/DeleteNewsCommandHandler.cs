using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Exceptions;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.News.Commands.DeleteNews;

public class DeleteNewsCommandHandler : IRequestHandler<DeleteNewsCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteNewsCommandHandler> _logger;

    public DeleteNewsCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<DeleteNewsCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteNewsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting news with ID: {NewsId}", request.NewsId);

        var news = await _unitOfWork.NewsRepository.GetByIdAsync(request.NewsId);
        if (news == null)
        {
            throw new NotFoundException($"News with ID {request.NewsId} not found");
        }

        await _unitOfWork.NewsRepository.DeleteAsync(request.NewsId);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("News deleted successfully: {NewsId}", request.NewsId);

        return Unit.Value;
    }
}
