using FluentValidation;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Requests.Commands.BulkApproveRequests.Validation;

public class BulkApproveRequestsCommandValidator : AbstractValidator<BulkApproveRequestsCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public BulkApproveRequestsCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.ApproverId)
            .NotEmpty()
            .WithMessage("Approver ID is required")
            .MustAsync(ApproverExists)
            .WithMessage("Approver not found");

        RuleFor(x => x.RequestStepIds)
            .NotEmpty()
            .WithMessage("At least one request step ID is required")
            .Must(x => x.Count <= 50)
            .WithMessage("Cannot approve more than 50 requests at once");

        RuleForEach(x => x.RequestStepIds)
            .NotEmpty()
            .WithMessage("Request step ID cannot be empty");

        RuleFor(x => x.Comment)
            .MaximumLength(2000)
            .WithMessage("Comment cannot exceed 2000 characters");
    }

    private async Task<bool> ApproverExists(Guid approverId, CancellationToken cancellationToken)
    {
        var approver = await _unitOfWork.UserRepository.GetByIdAsync(approverId);
        return approver != null && approver.IsActive;
    }
}