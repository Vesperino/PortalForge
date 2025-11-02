using FluentValidation;
using PortalForge.Domain.Entities;

namespace PortalForge.Application.UseCases.Locations.Commands.CreateCachedLocation.Validation;

public class CreateCachedLocationCommandValidator : AbstractValidator<CreateCachedLocationCommand>
{
    public CreateCachedLocationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required")
            .MaximumLength(500).WithMessage("Address cannot exceed 500 characters");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required")
            .Must(BeValidLocationType).WithMessage("Invalid location type. Must be: Office, ConferenceRoom, or Popular");

        RuleFor(x => x.CreatedBy)
            .NotEmpty().WithMessage("CreatedBy is required");
    }

    private bool BeValidLocationType(string type)
    {
        return Enum.TryParse<LocationType>(type, true, out _);
    }
}
