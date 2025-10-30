namespace PortalForge.Application.Common.Interfaces;

/// <summary>
/// Service for managing file storage operations.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Saves a file to storage
    /// </summary>
    /// <param name="fileStream">Stream of file content</param>
    /// <param name="fileName">Name of the file</param>
    /// <param name="category">Category/subdirectory (e.g., "news-images", "documents")</param>
    /// <returns>Relative path to the saved file</returns>
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string category);

    /// <summary>
    /// Deletes a file from storage
    /// </summary>
    /// <param name="relativePath">Relative path to the file</param>
    Task DeleteFileAsync(string relativePath);

    /// <summary>
    /// Checks if a file exists
    /// </summary>
    /// <param name="relativePath">Relative path to the file</param>
    Task<bool> FileExistsAsync(string relativePath);

    /// <summary>
    /// Gets the full physical path for a relative path
    /// </summary>
    /// <param name="relativePath">Relative path to the file</param>
    string GetFullPath(string relativePath);

    /// <summary>
    /// Gets storage settings from database
    /// </summary>
    Task<Dictionary<string, string>> GetStorageSettingsAsync();
}


