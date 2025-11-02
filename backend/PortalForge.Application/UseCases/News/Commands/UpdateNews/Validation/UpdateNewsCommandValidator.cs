using FluentValidation;
using PortalForge.Domain.Entities;

namespace PortalForge.Application.UseCases.News.Commands.UpdateNews.Validation;

public class UpdateNewsCommandValidator : AbstractValidator<UpdateNewsCommand>
{
    public UpdateNewsCommandValidator()
    {
        RuleFor(x => x.NewsId)
            .GreaterThan(0).WithMessage("News ID must be greater than 0");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(500).WithMessage("Title cannot exceed 500 characters");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required");

        RuleFor(x => x.Excerpt)
            .NotEmpty().WithMessage("Excerpt is required")
            .MaximumLength(1000).WithMessage("Excerpt cannot exceed 1000 characters");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(1000).WithMessage("Image URL cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .Must(BeValidCategory).WithMessage("Invalid news category. Valid categories: Company, HR, IT, Event, Social, Achievement, Training, General");

        RuleFor(x => x.EventId)
            .GreaterThan(0).WithMessage("Event ID must be greater than 0")
            .When(x => x.EventId.HasValue);
    }

    private bool BeValidCategory(string category)
    {
        return Enum.TryParse<NewsCategory>(category, true, out _);
    }
}
