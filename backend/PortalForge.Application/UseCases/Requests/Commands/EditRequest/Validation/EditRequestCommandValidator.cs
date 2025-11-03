using FluentValidation;

namespace PortalForge.Application.UseCases.Requests.Commands.EditRequest.Validation;

/// <summary>
/// Validator for EditRequestCommand.
/// Ensures form data is present and reason is within length limits.
/// </summary>
public class EditRequestCommandValidator : AbstractValidator<EditRequestCommand>
{
    public EditRequestCommandValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEmpty().WithMessage("ID wniosku jest wymagane");

        RuleFor(x => x.EditedByUserId)
            .NotEmpty().WithMessage("ID użytkownika edytującego jest wymagane");

        RuleFor(x => x.NewFormData)
            .NotEmpty().WithMessage("Dane formularza są wymagane")
            .MinimumLength(2).WithMessage("Dane formularza nie mogą być puste");

        RuleFor(x => x.ChangeReason)
            .MaximumLength(500).WithMessage("Powód zmiany nie może przekraczać 500 znaków");
    }
}
