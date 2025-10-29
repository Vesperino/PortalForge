using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Exceptions;
using PortalForge.Application.Common.Interfaces;

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

        news.Title = request.Title;
        news.Content = request.Content;
        news.Excerpt = request.Excerpt;
        news.ImageUrl = request.ImageUrl;
        news.Category = request.Category;
        news.EventId = request.EventId;
        news.IsEvent = request.IsEvent;
        news.EventHashtag = request.EventHashtag;
        news.EventDateTime = request.EventDateTime;
        news.EventLocation = request.EventLocation;
        news.DepartmentId = request.DepartmentId;
        news.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.NewsRepository.UpdateAsync(news);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("News updated successfully: {NewsId}", request.NewsId);

        return Unit.Value;
    }
}
