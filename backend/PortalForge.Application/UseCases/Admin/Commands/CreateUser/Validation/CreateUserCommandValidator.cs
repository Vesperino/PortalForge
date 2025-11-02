using FluentValidation;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Admin.Commands.CreateUser.Validation;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters")
            .MustAsync(BeUniqueEmail).WithMessage("Email already exists");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .MaximumLength(100).WithMessage("Password cannot exceed 100 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MinimumLength(2).WithMessage("First name must be at least 2 characters")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MinimumLength(2).WithMessage("Last name must be at least 2 characters")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters");

        RuleFor(x => x.Department)
            .NotEmpty().WithMessage("Department is required")
            .MaximumLength(200).WithMessage("Department cannot exceed 200 characters");

        When(x => x.DepartmentId.HasValue, () =>
        {
            RuleFor(x => x.DepartmentId!.Value)
                .MustAsync(DepartmentExists).WithMessage("Department does not exist");
        });

        RuleFor(x => x.Position)
            .NotEmpty().WithMessage("Position is required")
            .MaximumLength(200).WithMessage("Position cannot exceed 200 characters");

        When(x => !string.IsNullOrEmpty(x.PhoneNumber), () =>
        {
            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format");
        });

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required")
            .Must(BeValidRole).WithMessage("Role must be one of: Admin, HR, Manager, Marketing, Employee");

        RuleFor(x => x.CreatedBy)
            .NotEmpty().WithMessage("CreatedBy is required");
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        var existingUser = await _unitOfWork.UserRepository.GetByEmailAsync(email);
        return existingUser == null;
    }

    private async Task<bool> DepartmentExists(Guid departmentId, CancellationToken cancellationToken)
    {
        var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(departmentId);
        return department != null;
    }

    private bool BeValidRole(string role)
    {
        var validRoles = new[] { "Admin", "HR", "Manager", "Marketing", "Employee" };
        return validRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }
}
