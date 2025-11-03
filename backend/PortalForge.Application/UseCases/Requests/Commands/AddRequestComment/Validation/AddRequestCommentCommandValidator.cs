using FluentValidation;

namespace PortalForge.Application.UseCases.Requests.Commands.AddRequestComment.Validation;

/// <summary>
/// Validator for AddRequestCommentCommand.
/// Ensures request ID, user ID, and comment text are provided.
/// </summary>
public class AddRequestCommentCommandValidator : AbstractValidator<AddRequestCommentCommand>
{
    public AddRequestCommentCommandValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEmpty().WithMessage("ID wniosku jest wymagane");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("ID użytkownika jest wymagane");

        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Treść komentarza jest wymagana")
            .MinimumLength(2).WithMessage("Komentarz musi mieć co najmniej 2 znaki")
            .MaximumLength(2000).WithMessage("Komentarz nie może przekraczać 2000 znaków");

        RuleFor(x => x.Attachments)
            .MaximumLength(5000).WithMessage("Lista załączników jest zbyt długa")
            .When(x => !string.IsNullOrEmpty(x.Attachments));
    }
}
