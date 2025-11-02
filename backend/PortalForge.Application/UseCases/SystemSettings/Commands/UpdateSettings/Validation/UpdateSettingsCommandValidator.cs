using FluentValidation;

namespace PortalForge.Application.UseCases.SystemSettings.Commands.UpdateSettings.Validation;

public class UpdateSettingsCommandValidator : AbstractValidator<UpdateSettingsCommand>
{
    public UpdateSettingsCommandValidator()
    {
        RuleFor(x => x.Settings)
            .NotEmpty().WithMessage("At least one setting is required");

        RuleFor(x => x.UpdatedBy)
            .NotEmpty().WithMessage("UpdatedBy is required");

        RuleForEach(x => x.Settings).ChildRules(setting =>
        {
            setting.RuleFor(s => s.Key)
                .NotEmpty().WithMessage("Setting key is required")
                .MaximumLength(200).WithMessage("Key cannot exceed 200 characters");

            setting.RuleFor(s => s.Value)
                .NotNull().WithMessage("Setting value cannot be null")
                .MaximumLength(2000).WithMessage("Value cannot exceed 2000 characters");
        });
    }
}
