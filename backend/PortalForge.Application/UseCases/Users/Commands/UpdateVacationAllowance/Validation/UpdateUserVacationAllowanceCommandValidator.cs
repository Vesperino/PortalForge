using FluentValidation;

namespace PortalForge.Application.UseCases.Users.Commands.UpdateVacationAllowance.Validation;

/// <summary>
/// Validator for UpdateUserVacationAllowanceCommand.
/// Ensures vacation allowance is within valid range and reason is provided.
/// </summary>
public class UpdateUserVacationAllowanceCommandValidator : AbstractValidator<UpdateUserVacationAllowanceCommand>
{
    public UpdateUserVacationAllowanceCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("ID użytkownika jest wymagane");

        RuleFor(x => x.NewAnnualDays)
            .GreaterThan(0).WithMessage("Liczba dni urlopu musi być większa niż 0")
            .LessThanOrEqualTo(50).WithMessage("Liczba dni urlopu nie może przekraczać 50");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Powód zmiany jest wymagany")
            .MaximumLength(500).WithMessage("Powód nie może przekraczać 500 znaków");

        RuleFor(x => x.RequestedByUserId)
            .NotEmpty().WithMessage("ID użytkownika wykonującego operację jest wymagane");
    }
}
