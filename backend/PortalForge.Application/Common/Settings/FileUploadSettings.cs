namespace PortalForge.Application.Common.Settings;

/// <summary>
/// Configuration settings for file upload functionality.
/// </summary>
public class FileUploadSettings
{
    /// <summary>
    /// Configuration section name for binding from appsettings.
    /// </summary>
    public const string SectionName = "FileUpload";

    /// <summary>
    /// Maximum allowed file size in bytes.
    /// Default: 10 MB (10 * 1024 * 1024 bytes).
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;

    /// <summary>
    /// Allowed image file extensions.
    /// </summary>
    public string[] AllowedImageExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" };

    /// <summary>
    /// Allowed document file extensions.
    /// </summary>
    public string[] AllowedDocumentExtensions { get; set; } = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".csv" };

    /// <summary>
    /// Allowed attachment file extensions (includes images, documents, and archives).
    /// </summary>
    public string[] AllowedAttachmentExtensions { get; set; } =
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg",
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".csv", ".zip"
    };
}
