using FluentValidation;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Services;

namespace PortalForge.Application.UseCases.Requests.Commands.SubmitRequest.Validation;

public class SubmitRequestCommandValidator : AbstractValidator<SubmitRequestCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRequestRoutingService _routingService;

    public SubmitRequestCommandValidator(
        IUnitOfWork unitOfWork,
        IRequestRoutingService routingService)
    {
        _unitOfWork = unitOfWork;
        _routingService = routingService;

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

        // Validate approval structure using Custom to add multiple specific errors
        RuleFor(x => x)
            .CustomAsync(ValidateApprovalStructureAsync);
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

    private async Task ValidateApprovalStructureAsync(
        SubmitRequestCommand command,
        ValidationContext<SubmitRequestCommand> context,
        CancellationToken cancellationToken)
    {
        // Get the template with approval steps
        var template = await _unitOfWork.RequestTemplateRepository.GetByIdAsync(command.RequestTemplateId);
        if (template == null || !template.RequiresApproval)
        {
            return; // No approval required, validation passes
        }

        // Validate the approval structure
        var (isValid, errors) = await _routingService.ValidateApprovalStructureAsync(
            command.SubmittedById,
            template.ApprovalStepTemplates);

        if (!isValid)
        {
            // Add each error as a separate validation failure
            foreach (var error in errors)
            {
                context.AddFailure("ApprovalStructure", error);
            }
        }
    }
}
