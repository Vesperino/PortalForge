using FluentValidation;

namespace PortalForge.Application.UseCases.Storage.Commands.DeleteNewsImage.Validation;

public class DeleteNewsImageCommandValidator : AbstractValidator<DeleteNewsImageCommand>
{
    public DeleteNewsImageCommandValidator()
    {
        RuleFor(x => x.FilePath)
            .NotEmpty().WithMessage("File path is required")
            .MaximumLength(500).WithMessage("File path cannot exceed 500 characters")
            .Must(BeValidPath).WithMessage("Invalid file path - path traversal detected");
    }

    private bool BeValidPath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        // Prevent path traversal attacks
        if (filePath.Contains("..") ||
            filePath.Contains("\\") ||
            filePath.StartsWith("/") ||
            filePath.StartsWith("\\"))
        {
            return false;
        }

        return true;
    }
}
