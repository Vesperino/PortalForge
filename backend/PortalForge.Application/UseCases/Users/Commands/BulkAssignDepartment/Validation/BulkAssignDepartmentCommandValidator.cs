using FluentValidation;

namespace PortalForge.Application.UseCases.Users.Commands.BulkAssignDepartment.Validation;

/// <summary>
/// Validator for BulkAssignDepartmentCommand.
/// </summary>
public class BulkAssignDepartmentCommandValidator : AbstractValidator<BulkAssignDepartmentCommand>
{
    public BulkAssignDepartmentCommandValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("Department ID is required");

        RuleFor(x => x.EmployeeIds)
            .NotEmpty().WithMessage("At least one employee must be selected")
            .Must(list => list.Count > 0).WithMessage("Employee list cannot be empty")
            .Must(list => list.Count <= 100).WithMessage("Cannot assign more than 100 employees at once");

        RuleForEach(x => x.EmployeeIds)
            .NotEmpty().WithMessage("Employee ID cannot be empty");
    }
}
