using FluentValidation;

namespace PortalForge.Application.UseCases.Users.Commands.TransferDepartment.Validation;

/// <summary>
/// Validator for TransferEmployeeToDepartmentCommand.
/// Ensures required fields are provided and reason length is valid.
/// </summary>
public class TransferEmployeeToDepartmentCommandValidator : AbstractValidator<TransferEmployeeToDepartmentCommand>
{
    public TransferEmployeeToDepartmentCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("ID użytkownika jest wymagane");

        RuleFor(x => x.NewDepartmentId)
            .NotEmpty().WithMessage("ID nowego działu jest wymagane");

        RuleFor(x => x.TransferredByUserId)
            .NotEmpty().WithMessage("ID użytkownika wykonującego transfer jest wymagane");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Powód transferu nie może przekraczać 500 znaków")
            .When(x => !string.IsNullOrEmpty(x.Reason));

        RuleFor(x => x)
            .Must(x => x.UserId != x.NewSupervisorId)
            .WithMessage("Użytkownik nie może być swoim własnym przełożonym")
            .When(x => x.NewSupervisorId.HasValue);
    }
}
