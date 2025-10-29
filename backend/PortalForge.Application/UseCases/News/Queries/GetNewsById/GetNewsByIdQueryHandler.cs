using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Exceptions;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.News.DTOs;

namespace PortalForge.Application.UseCases.News.Queries.GetNewsById;

public class GetNewsByIdQueryHandler : IRequestHandler<GetNewsByIdQuery, NewsDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetNewsByIdQueryHandler> _logger;

    public GetNewsByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetNewsByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<NewsDto> Handle(GetNewsByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching news with ID: {NewsId}", request.NewsId);

        var news = await _unitOfWork.NewsRepository.GetByIdAsync(request.NewsId);
        if (news == null)
        {
            throw new NotFoundException($"News with ID {request.NewsId} not found");
        }

        await _unitOfWork.NewsRepository.IncrementViewsAsync(request.NewsId);
        await _unitOfWork.SaveChangesAsync();

        var newsDto = new NewsDto
        {
            Id = news.Id,
            Title = news.Title,
            Content = news.Content,
            Excerpt = news.Excerpt,
            ImageUrl = news.ImageUrl,
            AuthorId = news.AuthorId,
            AuthorName = news.Author != null ? news.Author.FullName : "Unknown",
            CreatedAt = news.CreatedAt,
            UpdatedAt = news.UpdatedAt,
            Views = news.Views + 1,
            Category = news.Category.ToString(),
            EventId = news.EventId
        };

        _logger.LogInformation("News fetched successfully: {NewsId}", request.NewsId);

        return newsDto;
    }
}
