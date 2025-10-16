using FluentValidation;

namespace PortalForge.Application.UseCases.Auth.Commands.Login.Validation;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email jest wymagany")
            .EmailAddress().WithMessage("Nieprawidłowy format email")
            .MaximumLength(255).WithMessage("Email nie może przekraczać 255 znaków");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Hasło jest wymagane")
            .MinimumLength(8).WithMessage("Hasło musi mieć minimum 8 znaków");
    }
}
