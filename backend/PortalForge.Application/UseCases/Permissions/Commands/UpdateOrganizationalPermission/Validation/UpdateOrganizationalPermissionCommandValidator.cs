using FluentValidation;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Permissions.Commands.UpdateOrganizationalPermission.Validation;

/// <summary>
/// Validator for UpdateOrganizationalPermissionCommand
/// </summary>
public class UpdateOrganizationalPermissionCommandValidator
    : AbstractValidator<UpdateOrganizationalPermissionCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateOrganizationalPermissionCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.UserId)
            .MustAsync(UserExists).WithMessage("User does not exist")
            .When(x => x.UserId != Guid.Empty);

        // Validate department IDs if not viewing all departments
        RuleFor(x => x.VisibleDepartmentIds)
            .MustAsync(AllDepartmentsExist)
            .WithMessage("One or more department IDs are invalid")
            .When(x => !x.CanViewAllDepartments && x.VisibleDepartmentIds.Any());
    }

    private async Task<bool> UserExists(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
        return user != null;
    }

    private async Task<bool> AllDepartmentsExist(
        List<Guid> departmentIds,
        CancellationToken cancellationToken)
    {
        if (departmentIds == null || !departmentIds.Any())
            return true;

        foreach (var departmentId in departmentIds)
        {
            var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(departmentId);
            if (department == null)
                return false;
        }

        return true;
    }
}
