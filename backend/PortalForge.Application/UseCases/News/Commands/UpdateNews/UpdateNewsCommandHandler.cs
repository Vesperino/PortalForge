using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Exceptions;
using PortalForge.Application.Common.Interfaces;
using System.Text.RegularExpressions;

namespace PortalForge.Application.UseCases.News.Commands.UpdateNews;

public class UpdateNewsCommandHandler : IRequestHandler<UpdateNewsCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUnifiedValidatorService _validatorService;
    private readonly ILogger<UpdateNewsCommandHandler> _logger;

    public UpdateNewsCommandHandler(
        IUnitOfWork unitOfWork,
        IUnifiedValidatorService validatorService,
        ILogger<UpdateNewsCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _validatorService = validatorService;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdateNewsCommand request, CancellationToken cancellationToken)
    {
        await _validatorService.ValidateAsync(request);

        _logger.LogInformation("Updating news with ID: {NewsId}", request.NewsId);

        var news = await _unitOfWork.NewsRepository.GetByIdAsync(request.NewsId);
        if (news == null)
        {
            throw new NotFoundException($"News with ID {request.NewsId} not found");
        }

        if (request.EventId.HasValue)
        {
            var eventEntity = await _unitOfWork.EventRepository.GetByIdAsync(request.EventId.Value);
            if (eventEntity == null)
            {
                throw new NotFoundException($"Event with ID {request.EventId.Value} not found");
            }
        }

        // Parse category string to enum (validator already checked it's valid)
        var category = Enum.Parse<Domain.Entities.NewsCategory>(request.Category, ignoreCase: true);

        news.Title = request.Title;
        news.Content = request.Content;
        news.Excerpt = request.Excerpt;
        news.ImageUrl = request.ImageUrl;
        news.Category = category;
        news.EventId = request.EventId;
        news.IsEvent = request.IsEvent;
        news.EventHashtag = request.EventHashtag;
        news.EventDateTime = request.EventDateTime;
        news.EventLocation = request.EventLocation;
        news.EventPlaceId = request.EventPlaceId;
        news.EventLatitude = request.EventLatitude;
        news.EventLongitude = request.EventLongitude;
        news.DepartmentId = request.DepartmentId;
        news.UpdatedAt = DateTime.UtcNow;

        // Handle hashtags - combine from request and auto-detect from content
        news.Hashtags.Clear();
        var allHashtags = await ProcessHashtagsAsync(request.Hashtags, request.Content);
        news.Hashtags = allHashtags;

        await _unitOfWork.NewsRepository.UpdateAsync(news);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("News updated successfully: {NewsId}", request.NewsId);

        return Unit.Value;
    }

    private async Task<List<Domain.Entities.Hashtag>> ProcessHashtagsAsync(List<string>? requestHashtags, string content)
    {
        var hashtagsFromRequest = requestHashtags ?? new List<string>();
        
        // Auto-detect hashtags from content using regex
        var hashtagPattern = @"(?:^|\s)(#[\w]+)";
        var matches = Regex.Matches(content, hashtagPattern);
        var hashtagsFromContent = matches.Select(m => m.Groups[1].Value).Distinct().ToList();

        // Combine both sources and remove duplicates (case-insensitive)
        var allHashtagNames = hashtagsFromRequest
            .Concat(hashtagsFromContent)
            .Select(h => h.StartsWith("#") ? h : $"#{h}")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (!allHashtagNames.Any())
        {
            return new List<Domain.Entities.Hashtag>();
        }

        // Get or create hashtags
        return await _unitOfWork.HashtagRepository.GetOrCreateHashtagsAsync(allHashtagNames);
    }
}
