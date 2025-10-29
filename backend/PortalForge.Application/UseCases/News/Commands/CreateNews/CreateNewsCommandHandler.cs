using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Exceptions;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.News.Commands.CreateNews;

public class CreateNewsCommandHandler : IRequestHandler<CreateNewsCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUnifiedValidatorService _validatorService;
    private readonly ILogger<CreateNewsCommandHandler> _logger;

    public CreateNewsCommandHandler(
        IUnitOfWork unitOfWork,
        IUnifiedValidatorService validatorService,
        ILogger<CreateNewsCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _validatorService = validatorService;
        _logger = logger;
    }

    public async Task<int> Handle(CreateNewsCommand request, CancellationToken cancellationToken)
    {
        await _validatorService.ValidateAsync(request);

        _logger.LogInformation(
            "Creating news: {Title}, Category: {Category}, AuthorId: {AuthorId}",
            request.Title, request.Category, request.AuthorId);

        var author = await _unitOfWork.UserRepository.GetByIdAsync(request.AuthorId);
        if (author == null)
        {
            throw new NotFoundException($"Author with ID {request.AuthorId} not found");
        }

        if (request.EventId.HasValue)
        {
            var eventEntity = await _unitOfWork.EventRepository.GetByIdAsync(request.EventId.Value);
            if (eventEntity == null)
            {
                throw new NotFoundException($"Event with ID {request.EventId.Value} not found");
            }
        }

        var news = new Domain.Entities.News
        {
            Title = request.Title,
            Content = request.Content,
            Excerpt = request.Excerpt,
            ImageUrl = request.ImageUrl,
            AuthorId = request.AuthorId,
            Category = request.Category,
            EventId = request.EventId,
            CreatedAt = DateTime.UtcNow,
            Views = 0
        };

        var newsId = await _unitOfWork.NewsRepository.CreateAsync(news);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("News created successfully with ID: {NewsId}", newsId);

        return newsId;
    }
}
