using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.News.DTOs;
using PortalForge.Domain.Entities;

namespace PortalForge.Application.UseCases.News.Queries.GetAllNews;

public class GetAllNewsQueryHandler : IRequestHandler<GetAllNewsQuery, IEnumerable<NewsDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAllNewsQueryHandler> _logger;

    public GetAllNewsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAllNewsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<NewsDto>> Handle(GetAllNewsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching all news, Category filter: {Category}", request.Category ?? "None");

        IEnumerable<Domain.Entities.News> newsList;

        if (!string.IsNullOrEmpty(request.Category) && Enum.TryParse<NewsCategory>(request.Category, true, out var category))
        {
            newsList = await _unitOfWork.NewsRepository.GetByCategoryAsync(category);
        }
        else
        {
            newsList = await _unitOfWork.NewsRepository.GetAllAsync();
        }

        var newsDto = newsList.Select(n => new NewsDto
        {
            Id = n.Id,
            Title = n.Title,
            Content = n.Content,
            Excerpt = n.Excerpt,
            ImageUrl = n.ImageUrl,
            AuthorId = n.AuthorId,
            AuthorName = n.Author != null ? n.Author.FullName : "Unknown",
            CreatedAt = n.CreatedAt,
            UpdatedAt = n.UpdatedAt,
            Views = n.Views,
            Category = n.Category.ToString(),
            EventId = n.EventId
        });

        _logger.LogInformation("Fetched {Count} news items", newsDto.Count());

        return newsDto;
    }
}
