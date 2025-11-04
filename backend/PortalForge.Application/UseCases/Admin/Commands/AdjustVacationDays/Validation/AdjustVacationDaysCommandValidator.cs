using FluentValidation;

namespace PortalForge.Application.UseCases.Admin.Commands.AdjustVacationDays.Validation;

public class AdjustVacationDaysCommandValidator : AbstractValidator<AdjustVacationDaysCommand>
{
    public AdjustVacationDaysCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.AdjustmentAmount)
            .NotEqual(0).WithMessage("Adjustment amount cannot be zero");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason for adjustment is required")
            .MinimumLength(10).WithMessage("Reason must be at least 10 characters")
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters");

        RuleFor(x => x.AdjustedBy)
            .NotEmpty().WithMessage("Admin user ID is required");
    }
}
