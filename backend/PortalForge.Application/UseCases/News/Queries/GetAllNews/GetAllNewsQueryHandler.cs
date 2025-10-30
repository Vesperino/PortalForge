using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.News.DTOs;
using PortalForge.Domain.Entities;

namespace PortalForge.Application.UseCases.News.Queries.GetAllNews;

public class GetAllNewsQueryHandler : IRequestHandler<GetAllNewsQuery, PaginatedNewsResponse>
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

    public async Task<PaginatedNewsResponse> Handle(GetAllNewsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Fetching news - Category: {Category}, DepartmentId: {DepartmentId}, IsEvent: {IsEvent}, Page: {PageNumber}, Size: {PageSize}",
            request.Category ?? "None",
            request.DepartmentId?.ToString() ?? "None",
            request.IsEvent?.ToString() ?? "None",
            request.PageNumber,
            request.PageSize);

        IEnumerable<Domain.Entities.News> allNews;

        if (!string.IsNullOrEmpty(request.Category) && Enum.TryParse<NewsCategory>(request.Category, true, out var category))
        {
            allNews = await _unitOfWork.NewsRepository.GetByCategoryAsync(category);
        }
        else
        {
            allNews = await _unitOfWork.NewsRepository.GetAllAsync();
        }

        // Apply department filter
        if (request.DepartmentId.HasValue)
        {
            allNews = allNews.Where(n => n.DepartmentId == null || n.DepartmentId == request.DepartmentId.Value);
        }

        // Apply event filter
        if (request.IsEvent.HasValue)
        {
            allNews = allNews.Where(n => n.IsEvent == request.IsEvent.Value);
        }

        var totalCount = allNews.Count();

        // Apply pagination
        var paginatedNews = allNews
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(n => new NewsDto
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
                EventId = n.EventId,
                IsEvent = n.IsEvent,
                EventHashtag = n.EventHashtag,
                EventDateTime = n.EventDateTime,
                EventLocation = n.EventLocation,
                DepartmentId = n.DepartmentId
            })
            .ToList();

        _logger.LogInformation(
            "Fetched {Count} of {TotalCount} news items (Page {PageNumber})",
            paginatedNews.Count,
            totalCount,
            request.PageNumber);

        return new PaginatedNewsResponse
        {
            Items = paginatedNews,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
