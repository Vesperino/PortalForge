using FluentValidation;

namespace PortalForge.Application.UseCases.Vacations.Commands.CancelVacation.Validation;

/// <summary>
/// Validator for CancelVacationCommand.
/// Ensures vacation schedule ID and reason are provided.
/// </summary>
public class CancelVacationCommandValidator : AbstractValidator<CancelVacationCommand>
{
    public CancelVacationCommandValidator()
    {
        RuleFor(x => x.VacationScheduleId)
            .NotEmpty().WithMessage("ID harmonogramu urlopu jest wymagane");

        RuleFor(x => x.CancelledByUserId)
            .NotEmpty().WithMessage("ID użytkownika anulującego urlop jest wymagane");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Powód anulowania urlopu jest wymagany")
            .MaximumLength(500).WithMessage("Powód nie może przekraczać 500 znaków");
    }
}
