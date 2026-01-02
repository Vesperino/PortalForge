using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Exceptions;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;

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
        _logger.LogInformation("Starting deletion of news with ID: {NewsId}", request.NewsId);

        var news = await _unitOfWork.NewsRepository.GetByIdAsync(request.NewsId);
        if (news == null)
        {
            _logger.LogWarning("News with ID {NewsId} not found - cannot delete", request.NewsId);
            throw new NotFoundException($"News with ID {request.NewsId} not found");
        }

        var currentUser = await _unitOfWork.UserRepository.GetByIdAsync(request.CurrentUserId);
        if (currentUser == null)
        {
            throw new NotFoundException($"User with ID {request.CurrentUserId} not found");
        }

        var isAdmin = currentUser.Role == UserRole.Admin;
        var isAuthor = news.AuthorId == request.CurrentUserId;

        if (!isAuthor && !isAdmin)
        {
            throw new ForbiddenException("Możesz modyfikować tylko własne aktualności lub musisz być administratorem");
        }

        _logger.LogInformation("Found news to delete - ID: {NewsId}, Title: {Title}, HasEvent: {HasEvent}, HashtagCount: {HashtagCount}",
            request.NewsId,
            news.Title,
            news.EventId.HasValue,
            news.Hashtags?.Count ?? 0);

        await _unitOfWork.NewsRepository.DeleteAsync(request.NewsId);
        _logger.LogInformation("News marked for deletion in context: {NewsId}", request.NewsId);

        var changes = await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("SaveChanges completed - News ID: {NewsId}, Changes saved: {ChangeCount}", request.NewsId, changes);

        // Verify deletion
        var deletedNews = await _unitOfWork.NewsRepository.GetByIdAsync(request.NewsId);
        if (deletedNews != null)
        {
            _logger.LogError("CRITICAL: News still exists after deletion! ID: {NewsId}", request.NewsId);
        }
        else
        {
            _logger.LogInformation("Verified: News successfully deleted from database - ID: {NewsId}", request.NewsId);
        }

        return Unit.Value;
    }
}
