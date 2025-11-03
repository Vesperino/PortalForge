namespace PortalForge.Application.Interfaces;

/// <summary>
/// Service for managing file storage operations.
/// Handles file uploads, downloads, validation, and deletion.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Saves a file to storage and returns its relative path.
    /// </summary>
    /// <param name="fileStream">The file stream to save.</param>
    /// <param name="fileName">Original file name (will be sanitized).</param>
    /// <param name="subfolder">Subfolder within uploads directory (e.g., "request-attachments", "sick-leaves").</param>
    /// <returns>Relative path to the saved file (e.g., "request-attachments/2025-01-03/abc123-document.pdf").</returns>
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string subfolder);

    /// <summary>
    /// Deletes a file from storage.
    /// </summary>
    /// <param name="relativePath">Relative path to the file.</param>
    Task DeleteFileAsync(string relativePath);

    /// <summary>
    /// Gets a file stream for download.
    /// </summary>
    /// <param name="relativePath">Relative path to the file.</param>
    /// <returns>File stream, or null if file doesn't exist.</returns>
    Task<Stream?> GetFileAsync(string relativePath);

    /// <summary>
    /// Gets absolute URL for a file (for frontend display/download).
    /// </summary>
    /// <param name="relativePath">Relative path to the file.</param>
    /// <returns>Absolute URL to access the file.</returns>
    string GetFileUrl(string relativePath);

    /// <summary>
    /// Validates file extension and size before upload.
    /// </summary>
    /// <param name="stream">File stream to validate.</param>
    /// <param name="fileName">File name (for extension check).</param>
    /// <param name="allowedExtensions">Allowed file extensions (e.g., [".pdf", ".jpg", ".png"]).</param>
    /// <param name="maxSizeBytes">Maximum allowed file size in bytes.</param>
    /// <returns>Tuple indicating if file is valid and error message if not.</returns>
    Task<(bool IsValid, string? ErrorMessage)> ValidateFileAsync(
        Stream stream,
        string fileName,
        string[] allowedExtensions,
        long maxSizeBytes);

    /// <summary>
    /// Checks if a file exists at the specified path.
    /// </summary>
    /// <param name="relativePath">Relative path to check.</param>
    /// <returns>True if file exists, false otherwise.</returns>
    Task<bool> FileExistsAsync(string relativePath);
}
