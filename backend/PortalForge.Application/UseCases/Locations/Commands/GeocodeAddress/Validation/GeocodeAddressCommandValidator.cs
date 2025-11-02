using FluentValidation;

namespace PortalForge.Application.UseCases.Locations.Commands.GeocodeAddress.Validation;

public class GeocodeAddressCommandValidator : AbstractValidator<GeocodeAddressCommand>
{
    public GeocodeAddressCommandValidator()
    {
        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required")
            .MinimumLength(3).WithMessage("Address must be at least 3 characters")
            .MaximumLength(500).WithMessage("Address cannot exceed 500 characters");
    }
}
