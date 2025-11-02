using FluentValidation;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Requests.Commands.RejectRequestStep.Validation;

public class RejectRequestStepCommandValidator : AbstractValidator<RejectRequestStepCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public RejectRequestStepCommandValidator(IUnitOfWork unitOfWork)
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

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Rejection reason is required")
            .MinimumLength(10).WithMessage("Rejection reason must be at least 10 characters")
            .MaximumLength(1000).WithMessage("Rejection reason cannot exceed 1000 characters");
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
