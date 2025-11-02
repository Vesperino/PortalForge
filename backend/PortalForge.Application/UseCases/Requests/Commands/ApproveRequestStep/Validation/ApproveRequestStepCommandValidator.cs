using FluentValidation;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Requests.Commands.ApproveRequestStep.Validation;

public class ApproveRequestStepCommandValidator : AbstractValidator<ApproveRequestStepCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public ApproveRequestStepCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.RequestId)
            .NotEmpty().WithMessage("Request ID is required")
            .MustAsync(RequestExists).WithMessage("Request does not exist");

        RuleFor(x => x.StepId)
            .NotEmpty().WithMessage("Step ID is required");

        RuleFor(x => x.ApproverId)
            .NotEmpty().WithMessage("Approver ID is required")
            .MustAsync(UserExists).WithMessage("Approver does not exist");

        When(x => !string.IsNullOrEmpty(x.Comment), () =>
        {
            RuleFor(x => x.Comment)
                .MaximumLength(1000).WithMessage("Comment cannot exceed 1000 characters");
        });
    }

    private async Task<bool> RequestExists(Guid requestId, CancellationToken cancellationToken)
    {
        var request = await _unitOfWork.RequestRepository.GetByIdAsync(requestId);
        return request != null;
    }

    private async Task<bool> UserExists(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
        return user != null;
    }
}
