using FluentValidation;

namespace PortalForge.Application.UseCases.Storage.Commands.UploadUserAvatar.Validation;

public class UploadUserAvatarCommandValidator : AbstractValidator<UploadUserAvatarCommand>
{
    private const int MaxFileSizeMB = 5;
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    public UploadUserAvatarCommandValidator()
    {
        RuleFor(x => x.FileStream)
            .NotNull().WithMessage("File is required");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required")
            .MaximumLength(255).WithMessage("File name cannot exceed 255 characters")
            .Must(HaveValidExtension).WithMessage("Invalid file type. Allowed types: JPG, JPEG, PNG, GIF, WebP");

        RuleFor(x => x.FileSize)
            .GreaterThan(0).WithMessage("File cannot be empty")
            .Must(HaveValidSize).WithMessage($"File size must not exceed {MaxFileSizeMB}MB");

        RuleFor(x => x.UploadedBy)
            .NotEmpty().WithMessage("User ID is required");
    }

    private bool HaveValidSize(long fileSize)
    {
        var maxFileSize = MaxFileSizeMB * 1024 * 1024;
        return fileSize <= maxFileSize;
    }

    private bool HaveValidExtension(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return false;

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return AllowedExtensions.Contains(extension);
    }
}
