using FluentValidation;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Departments.Commands.DeleteDepartment.Validation;

/// <summary>
/// Validator for DeleteDepartmentCommand.
/// </summary>
public class DeleteDepartmentCommandValidator : AbstractValidator<DeleteDepartmentCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDepartmentCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("Department ID is required");

        RuleFor(x => x.DepartmentId)
            .MustAsync(DepartmentExists).WithMessage("Department does not exist")
            .When(x => x.DepartmentId != Guid.Empty);
    }

    private async Task<bool> DepartmentExists(Guid departmentId, CancellationToken cancellationToken)
    {
        var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(departmentId);
        return department != null;
    }
}
