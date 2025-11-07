using FluentValidation;

namespace PortalForge.Application.UseCases.Profile.Commands.UpdateMyProfile.Validation;

public class UpdateMyProfileCommandValidator : AbstractValidator<UpdateMyProfileCommand>
{
    public UpdateMyProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.ProfilePhotoUrl)
            .MaximumLength(500).WithMessage("Profile photo URL cannot exceed 500 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.ProfilePhotoUrl));
    }
}
