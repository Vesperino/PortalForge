using FluentValidation;

namespace PortalForge.Application.UseCases.News.Commands.CreateNews.Validation;

public class CreateNewsCommandValidator : AbstractValidator<CreateNewsCommand>
{
    public CreateNewsCommandValidator()
    {
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

        RuleFor(x => x.AuthorId)
            .NotEmpty().WithMessage("Author ID is required");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Invalid news category");

        RuleFor(x => x.EventId)
            .GreaterThan(0).WithMessage("Event ID must be greater than 0")
            .When(x => x.EventId.HasValue);
    }
}
