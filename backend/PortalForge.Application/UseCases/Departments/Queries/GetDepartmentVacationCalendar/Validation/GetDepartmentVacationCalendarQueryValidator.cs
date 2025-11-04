using FluentValidation;

namespace PortalForge.Application.UseCases.Departments.Queries.GetDepartmentVacationCalendar.Validation;

/// <summary>
/// Validator for GetDepartmentVacationCalendarQuery.
/// Ensures department ID and date range are valid.
/// </summary>
public class GetDepartmentVacationCalendarQueryValidator : AbstractValidator<GetDepartmentVacationCalendarQuery>
{
    public GetDepartmentVacationCalendarQueryValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("ID działu jest wymagane");

        RuleFor(x => x.FromDate)
            .NotEmpty().WithMessage("Data początkowa jest wymagana");

        RuleFor(x => x.ToDate)
            .NotEmpty().WithMessage("Data końcowa jest wymagana")
            .GreaterThanOrEqualTo(x => x.FromDate).WithMessage("Data końcowa musi być późniejsza lub równa dacie początkowej");

        RuleFor(x => x)
            .Must(x => (x.ToDate - x.FromDate).TotalDays <= 365)
            .WithMessage("Zakres dat nie może przekraczać 365 dni");
    }
}
