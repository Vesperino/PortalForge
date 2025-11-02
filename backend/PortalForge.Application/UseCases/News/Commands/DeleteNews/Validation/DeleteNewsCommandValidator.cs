using FluentValidation;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.News.Commands.DeleteNews.Validation;

public class DeleteNewsCommandValidator : AbstractValidator<DeleteNewsCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteNewsCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.NewsId)
            .GreaterThan(0).WithMessage("News ID must be greater than 0")
            .MustAsync(NewsExists).WithMessage("News article does not exist");
    }

    private async Task<bool> NewsExists(int newsId, CancellationToken cancellationToken)
    {
        var news = await _unitOfWork.NewsRepository.GetByIdAsync(newsId);
        return news != null;
    }
}
