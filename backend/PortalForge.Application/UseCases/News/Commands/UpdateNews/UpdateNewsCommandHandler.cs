using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Exceptions;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;
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

        // Handle hashtags - combine from request, auto-detect from content, and include eventHashtag
        news.Hashtags.Clear();
        var allHashtags = await ProcessHashtagsAsync(request.Hashtags, request.Content, request.EventHashtag);
        news.Hashtags = allHashtags;

        await _unitOfWork.NewsRepository.UpdateAsync(news);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("News updated successfully: {NewsId}", request.NewsId);

        return Unit.Value;
    }

    private async Task<List<Domain.Entities.Hashtag>> ProcessHashtagsAsync(List<string>? requestHashtags, string content, string? eventHashtag)
    {
        var hashtagsFromRequest = requestHashtags ?? new List<string>();

        // Auto-detect hashtags from content using regex
        var hashtagPattern = @"(?:^|\s)(#[\w]+)";
        var matches = Regex.Matches(content, hashtagPattern);
        var hashtagsFromContent = matches.Select(m => m.Groups[1].Value).Distinct().ToList();

        // Include eventHashtag if provided
        var hashtagsToProcess = hashtagsFromRequest.Concat(hashtagsFromContent).ToList();
        if (!string.IsNullOrWhiteSpace(eventHashtag))
        {
            hashtagsToProcess.Add(eventHashtag);
        }

        // Combine all sources and remove duplicates (case-insensitive)
        var allHashtagNames = hashtagsToProcess
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
