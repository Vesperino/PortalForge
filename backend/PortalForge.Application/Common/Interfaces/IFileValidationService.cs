namespace PortalForge.Application.Common.Interfaces;

/// <summary>
/// Service for validating file uploads.
/// </summary>
public interface IFileValidationService
{
    /// <summary>
    /// Validates a file for image upload (news images, service icons).
    /// </summary>
    /// <param name="fileName">Name of the file including extension</param>
    /// <param name="fileSize">Size of the file in bytes</param>
    /// <returns>Validation result with success flag and optional error message</returns>
    FileValidationResult ValidateImageUpload(string? fileName, long fileSize);

    /// <summary>
    /// Validates a file for attachment upload (comments, requests).
    /// </summary>
    /// <param name="fileName">Name of the file including extension</param>
    /// <param name="fileSize">Size of the file in bytes</param>
    /// <returns>Validation result with success flag and optional error message</returns>
    FileValidationResult ValidateAttachmentUpload(string? fileName, long fileSize);

    /// <summary>
    /// Validates file size against configured maximum.
    /// </summary>
    /// <param name="fileSize">File size in bytes</param>
    /// <returns>True if file size is within limits</returns>
    bool IsFileSizeValid(long fileSize);

    /// <summary>
    /// Checks if file extension is allowed for images.
    /// </summary>
    /// <param name="fileName">File name with extension</param>
    /// <returns>True if extension is allowed</returns>
    bool IsImageExtensionAllowed(string fileName);

    /// <summary>
    /// Checks if file extension is allowed for attachments.
    /// </summary>
    /// <param name="fileName">File name with extension</param>
    /// <returns>True if extension is allowed</returns>
    bool IsAttachmentExtensionAllowed(string fileName);

    /// <summary>
    /// Gets the maximum allowed file size in bytes.
    /// </summary>
    long MaxFileSizeBytes { get; }

    /// <summary>
    /// Gets a human-readable representation of the max file size.
    /// </summary>
    string MaxFileSizeDisplay { get; }
}

/// <summary>
/// Result of file validation.
/// </summary>
public class FileValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }

    public static FileValidationResult Success() => new() { IsValid = true };

    public static FileValidationResult Failure(string errorMessage) => new()
    {
        IsValid = false,
        ErrorMessage = errorMessage
    };
}
