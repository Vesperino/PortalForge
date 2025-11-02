using FluentValidation;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.RequestTemplates.Commands.UpdateRequestTemplate.Validation;

public class UpdateRequestTemplateCommandValidator : AbstractValidator<UpdateRequestTemplateCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRequestTemplateCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Template ID is required")
            .MustAsync(TemplateExists).WithMessage("Template does not exist");

        When(x => !string.IsNullOrEmpty(x.Name), () =>
        {
            RuleFor(x => x.Name)
                .MinimumLength(3).WithMessage("Template name must be at least 3 characters")
                .MaximumLength(200).WithMessage("Template name cannot exceed 200 characters");
        });

        When(x => !string.IsNullOrEmpty(x.Description), () =>
        {
            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");
        });

        When(x => !string.IsNullOrEmpty(x.Icon), () =>
        {
            RuleFor(x => x.Icon)
                .MaximumLength(100).WithMessage("Icon cannot exceed 100 characters");
        });

        When(x => !string.IsNullOrEmpty(x.Category), () =>
        {
            RuleFor(x => x.Category)
                .MaximumLength(100).WithMessage("Category cannot exceed 100 characters")
                .Must(BeValidCategory).WithMessage("Category must be one of: General, HR, IT, Finance, Operations, Other");
        });

        When(x => !string.IsNullOrEmpty(x.DepartmentId), () =>
        {
            RuleFor(x => x.DepartmentId)
                .Must(BeValidGuid).WithMessage("Invalid department ID format");
        });

        When(x => x.EstimatedProcessingDays.HasValue, () =>
        {
            RuleFor(x => x.EstimatedProcessingDays!.Value)
                .GreaterThan(0).WithMessage("Estimated processing days must be greater than 0")
                .LessThanOrEqualTo(365).WithMessage("Estimated processing days cannot exceed 365");
        });

        When(x => x.PassingScore.HasValue, () =>
        {
            RuleFor(x => x.PassingScore!.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Passing score must be greater than or equal to 0")
                .LessThanOrEqualTo(100).WithMessage("Passing score cannot exceed 100");
        });

        When(x => x.Fields != null, () =>
        {
            RuleForEach(x => x.Fields).ChildRules(field =>
            {
                field.RuleFor(f => f.Label)
                    .NotEmpty().WithMessage("Field label is required")
                    .MaximumLength(200).WithMessage("Field label cannot exceed 200 characters");

                field.RuleFor(f => f.FieldType)
                    .NotEmpty().WithMessage("Field type is required")
                    .Must(BeValidFieldType).WithMessage("Invalid field type");
            });
        });

        When(x => x.ApprovalStepTemplates != null, () =>
        {
            RuleForEach(x => x.ApprovalStepTemplates).ChildRules(step =>
            {
                step.RuleFor(s => s.StepOrder)
                    .GreaterThan(0).WithMessage("Step order must be greater than 0");

                step.RuleFor(s => s.ApproverType)
                    .NotEmpty().WithMessage("Approver type is required")
                    .Must(BeValidApproverType).WithMessage("Approver type must be one of: Role, SpecificUser, UserGroup, Submitter");
            });
        });

        When(x => x.QuizQuestions != null, () =>
        {
            RuleForEach(x => x.QuizQuestions).ChildRules(question =>
            {
                question.RuleFor(q => q.Question)
                    .NotEmpty().WithMessage("Question text is required")
                    .MaximumLength(500).WithMessage("Question text cannot exceed 500 characters");

                question.RuleFor(q => q.Options)
                    .NotEmpty().WithMessage("Question must have at least one option");
            });
        });
    }

    private async Task<bool> TemplateExists(Guid templateId, CancellationToken cancellationToken)
    {
        var template = await _unitOfWork.RequestTemplateRepository.GetByIdAsync(templateId);
        return template != null;
    }

    private bool BeValidCategory(string? category)
    {
        if (string.IsNullOrEmpty(category)) return false;

        var validCategories = new[] { "General", "HR", "IT", "Finance", "Operations", "Other" };
        return validCategories.Contains(category, StringComparer.OrdinalIgnoreCase);
    }

    private bool BeValidGuid(string? guidString)
    {
        if (string.IsNullOrEmpty(guidString))
            return false;

        return Guid.TryParse(guidString, out _);
    }

    private bool BeValidFieldType(string fieldType)
    {
        var validTypes = new[] { "Text", "Number", "Date", "Email", "Phone", "Dropdown", "Checkbox", "TextArea", "File" };
        return validTypes.Contains(fieldType, StringComparer.OrdinalIgnoreCase);
    }

    private bool BeValidApproverType(string approverType)
    {
        var validTypes = new[] { "Role", "SpecificUser", "UserGroup", "Submitter" };
        return validTypes.Contains(approverType, StringComparer.OrdinalIgnoreCase);
    }
}
