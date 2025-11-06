using FluentValidation;

namespace PortalForge.Application.UseCases.Requests.Commands.AddRequestComment.Validation;

/// <summary>
/// Validator for AddRequestCommentCommand.
/// Ensures request ID, user ID are provided, and either comment text or attachments are included.
/// </summary>
public class AddRequestCommentCommandValidator : AbstractValidator<AddRequestCommentCommand>
{
    public AddRequestCommentCommandValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEmpty().WithMessage("ID wniosku jest wymagane");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("ID użytkownika jest wymagane");

        // Comment is optional if attachments are provided
        RuleFor(x => x.Comment)
            .MinimumLength(2).WithMessage("Komentarz musi mieć co najmniej 2 znaki")
            .MaximumLength(2000).WithMessage("Komentarz nie może przekraczać 2000 znaków")
            .When(x => !string.IsNullOrWhiteSpace(x.Comment));

        RuleFor(x => x.Attachments)
            .MaximumLength(5000).WithMessage("Lista załączników jest zbyt długa")
            .When(x => !string.IsNullOrEmpty(x.Attachments));

        // At least one of comment or attachments must be provided
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Comment) || !string.IsNullOrWhiteSpace(x.Attachments))
            .WithMessage("Musisz podać tekst komentarza lub załącznik");
    }
}
