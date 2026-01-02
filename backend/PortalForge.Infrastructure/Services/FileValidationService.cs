using Microsoft.Extensions.Options;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Common.Settings;

namespace PortalForge.Infrastructure.Services;

/// <summary>
/// Service for validating file uploads.
/// Uses configuration from FileUploadSettings.
/// </summary>
public class FileValidationService : IFileValidationService
{
    private readonly FileUploadSettings _settings;
    private readonly HashSet<string> _allowedImageExtensions;
    private readonly HashSet<string> _allowedAttachmentExtensions;

    public FileValidationService(IOptions<FileUploadSettings> settings)
    {
        _settings = settings.Value;
        _allowedImageExtensions = new HashSet<string>(
            _settings.AllowedImageExtensions,
            StringComparer.OrdinalIgnoreCase);
        _allowedAttachmentExtensions = new HashSet<string>(
            _settings.AllowedAttachmentExtensions,
            StringComparer.OrdinalIgnoreCase);
    }

    public long MaxFileSizeBytes => _settings.MaxFileSizeBytes;

    public string MaxFileSizeDisplay
    {
        get
        {
            var sizeInMb = _settings.MaxFileSizeBytes / (1024.0 * 1024.0);
            return $"{sizeInMb:0.##}MB";
        }
    }

    public FileValidationResult ValidateImageUpload(string? fileName, long fileSize)
    {
        if (string.IsNullOrEmpty(fileName) || fileSize == 0)
        {
            return FileValidationResult.Failure("No file provided");
        }

        if (!IsFileSizeValid(fileSize))
        {
            return FileValidationResult.Failure($"File size exceeds maximum allowed ({MaxFileSizeDisplay})");
        }

        if (!IsImageExtensionAllowed(fileName))
        {
            var extensions = string.Join(", ", _settings.AllowedImageExtensions.Select(e => e.TrimStart('.')));
            return FileValidationResult.Failure($"Invalid file type. Allowed: {extensions}");
        }

        return FileValidationResult.Success();
    }

    public FileValidationResult ValidateAttachmentUpload(string? fileName, long fileSize)
    {
        if (string.IsNullOrEmpty(fileName) || fileSize == 0)
        {
            return FileValidationResult.Failure("No file provided");
        }

        if (!IsFileSizeValid(fileSize))
        {
            return FileValidationResult.Failure($"File size exceeds maximum allowed ({MaxFileSizeDisplay})");
        }

        if (!IsAttachmentExtensionAllowed(fileName))
        {
            return FileValidationResult.Failure("Invalid file type. Allowed: images, documents (pdf, doc, docx, xls, xlsx, txt, csv), zip");
        }

        return FileValidationResult.Success();
    }

    public bool IsFileSizeValid(long fileSize)
    {
        return fileSize <= _settings.MaxFileSizeBytes;
    }

    public bool IsImageExtensionAllowed(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        return !string.IsNullOrEmpty(extension) && _allowedImageExtensions.Contains(extension);
    }

    public bool IsAttachmentExtensionAllowed(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        return !string.IsNullOrEmpty(extension) && _allowedAttachmentExtensions.Contains(extension);
    }
}
