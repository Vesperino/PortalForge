using FluentValidation;

namespace PortalForge.Application.UseCases.Storage.Commands.UploadServiceIcon.Validation;

public class UploadServiceIconCommandValidator : AbstractValidator<UploadServiceIconCommand>
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".svg", ".webp", ".ico" };

    public UploadServiceIconCommandValidator()
    {
        RuleFor(x => x.FileStream)
            .NotNull()
            .WithMessage("File stream is required");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("File name is required")
            .Must(HaveAllowedExtension)
            .WithMessage($"File must be one of the following types: {string.Join(", ", AllowedExtensions)}");

        RuleFor(x => x.FileSize)
            .GreaterThan(0)
            .WithMessage("File size must be greater than 0")
            .LessThanOrEqualTo(MaxFileSizeBytes)
            .WithMessage($"File size must not exceed {MaxFileSizeBytes / 1024 / 1024} MB");
    }

    private bool HaveAllowedExtension(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return AllowedExtensions.Contains(extension);
    }
}
