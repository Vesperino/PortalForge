using FluentValidation;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Requests.Commands.SubmitRequest.Validation;

public class SubmitRequestCommandValidator : AbstractValidator<SubmitRequestCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public SubmitRequestCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.RequestTemplateId)
            .NotEmpty().WithMessage("Request template ID is required")
            .MustAsync(RequestTemplateExists).WithMessage("Request template does not exist");

        RuleFor(x => x.SubmittedById)
            .NotEmpty().WithMessage("Submitted by user ID is required")
            .MustAsync(UserExists).WithMessage("User does not exist");

        RuleFor(x => x.Priority)
            .NotEmpty().WithMessage("Priority is required")
            .Must(BeValidPriority).WithMessage("Priority must be one of: Low, Standard, High, Urgent");

        RuleFor(x => x.FormData)
            .NotEmpty().WithMessage("Form data is required")
            .MaximumLength(10000).WithMessage("Form data cannot exceed 10000 characters")
            .Must(BeValidJson).WithMessage("Form data must be valid JSON");
    }

    private async Task<bool> RequestTemplateExists(Guid requestTemplateId, CancellationToken cancellationToken)
    {
        var template = await _unitOfWork.RequestTemplateRepository.GetByIdAsync(requestTemplateId);
        return template != null;
    }

    private async Task<bool> UserExists(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
        return user != null;
    }

    private bool BeValidPriority(string priority)
    {
        var validPriorities = new[] { "Low", "Standard", "High", "Urgent" };
        return validPriorities.Contains(priority, StringComparer.OrdinalIgnoreCase);
    }

    private bool BeValidJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            System.Text.Json.JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
