using FluentValidation;

namespace PortalForge.Application.UseCases.SystemSettings.Queries.GetSettingByKey.Validation;

public class GetSettingByKeyQueryValidator : AbstractValidator<GetSettingByKeyQuery>
{
    public GetSettingByKeyQueryValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Key is required")
            .MaximumLength(200).WithMessage("Key cannot exceed 200 characters");
    }
}
