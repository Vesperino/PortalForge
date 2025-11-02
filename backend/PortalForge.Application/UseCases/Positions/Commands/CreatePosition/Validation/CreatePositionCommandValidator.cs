using FluentValidation;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Positions.Commands.CreatePosition.Validation;

public class CreatePositionCommandValidator : AbstractValidator<CreatePositionCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreatePositionCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Position name is required")
            .MaximumLength(200).WithMessage("Position name cannot exceed 200 characters")
            .MustAsync(BeUniqueName).WithMessage("A position with this name already exists");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");
    }

    private async Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
    {
        var existingPosition = await _unitOfWork.PositionRepository.GetByNameAsync(name);
        return existingPosition == null;
    }
}
