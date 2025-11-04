using FluentValidation;

namespace PortalForge.Application.UseCases.Users.Commands.UpdateFullVacationData.Validation;

public class UpdateFullVacationDataCommandValidator : AbstractValidator<UpdateFullVacationDataCommand>
{
    public UpdateFullVacationDataCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID jest wymagane");

        RuleFor(x => x.AnnualVacationDays)
            .GreaterThanOrEqualTo(0).WithMessage("Roczny wymiar urlopu nie może być ujemny")
            .LessThanOrEqualTo(50).WithMessage("Roczny wymiar urlopu nie może przekraczać 50 dni");

        RuleFor(x => x.VacationDaysUsed)
            .GreaterThanOrEqualTo(0).WithMessage("Wykorzystane dni nie mogą być ujemne");

        RuleFor(x => x.OnDemandVacationDaysUsed)
            .GreaterThanOrEqualTo(0).WithMessage("Wykorzystane dni na żądanie nie mogą być ujemne")
            .LessThanOrEqualTo(4).WithMessage("Maksymalnie 4 dni urlopu na żądanie rocznie");

        RuleFor(x => x.CircumstantialLeaveDaysUsed)
            .GreaterThanOrEqualTo(0).WithMessage("Wykorzystane dni okolicznościowe nie mogą być ujemne");

        RuleFor(x => x.CarriedOverVacationDays)
            .GreaterThanOrEqualTo(0).WithMessage("Dni przeniesione nie mogą być ujemne");

        RuleFor(x => x)
            .Must(cmd => cmd.VacationDaysUsed <= cmd.AnnualVacationDays + cmd.CarriedOverVacationDays)
            .WithMessage("Wykorzystane dni nie mogą przekraczać dostępnych dni");

        RuleFor(x => x.RequestedByUserId)
            .NotEmpty().WithMessage("ID osoby aktualizującej jest wymagane");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Powód aktualizacji jest wymagany")
            .MaximumLength(500).WithMessage("Powód nie może przekraczać 500 znaków");
    }
}
