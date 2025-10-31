using FluentValidation;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Departments.Commands.UpdateDepartment.Validation;

/// <summary>
/// Validator for UpdateDepartmentCommand.
/// </summary>
public class UpdateDepartmentCommandValidator : AbstractValidator<UpdateDepartmentCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDepartmentCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("Department ID is required")
            .MustAsync(DepartmentExists).WithMessage("Department does not exist");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Department name is required")
            .MaximumLength(200).WithMessage("Department name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.ParentDepartmentId)
            .MustAsync(ParentDepartmentExists)
            .WithMessage("Parent department does not exist")
            .When(x => x.ParentDepartmentId.HasValue)
            .Must((command, parentId) => parentId != command.DepartmentId)
            .WithMessage("Department cannot be its own parent")
            .When(x => x.ParentDepartmentId.HasValue);

        RuleFor(x => x.DepartmentHeadId)
            .MustAsync(UserExists)
            .WithMessage("Department head user does not exist")
            .When(x => x.DepartmentHeadId.HasValue);
    }

    private async Task<bool> DepartmentExists(Guid departmentId, CancellationToken cancellationToken)
    {
        var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(departmentId);
        return department != null;
    }

    private async Task<bool> ParentDepartmentExists(Guid? parentId, CancellationToken cancellationToken)
    {
        if (!parentId.HasValue) return true;

        var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(parentId.Value);
        return department != null && department.IsActive;
    }

    private async Task<bool> UserExists(Guid? userId, CancellationToken cancellationToken)
    {
        if (!userId.HasValue) return true;

        var user = await _unitOfWork.UserRepository.GetByIdAsync(userId.Value);
        return user != null && user.IsActive;
    }
}
