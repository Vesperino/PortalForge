using FluentValidation;

namespace PortalForge.Application.UseCases.Auth.Commands.RefreshToken.Validation;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required")
            .MinimumLength(10).WithMessage("Invalid refresh token format");
    }
}
