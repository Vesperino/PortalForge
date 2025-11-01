using FluentValidation;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Positions.Commands.UpdatePosition.Validation;

public class UpdatePositionCommandValidator : AbstractValidator<UpdatePositionCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePositionCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.PositionId)
            .NotEmpty().WithMessage("Position ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Position name is required")
            .MaximumLength(200).WithMessage("Position name cannot exceed 200 characters")
            .MustAsync(BeUniqueNameForUpdate).WithMessage("A position with this name already exists");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");
    }

    private async Task<bool> BeUniqueNameForUpdate(UpdatePositionCommand command, string name, CancellationToken cancellationToken)
    {
        var existingPosition = await _unitOfWork.PositionRepository.GetByNameAsync(name);
        return existingPosition == null || existingPosition.Id == command.PositionId;
    }
}
