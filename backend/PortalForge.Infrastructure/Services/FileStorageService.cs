using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Interfaces;

namespace PortalForge.Infrastructure.Services;

/// <summary>
/// Service for managing file storage on local file system.
/// Stores files in configured uploads directory with date-based organization.
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<FileStorageService> _logger;
    private readonly string _uploadsPath;
    private readonly string _baseUrl;

    public FileStorageService(
        IConfiguration configuration,
        ILogger<FileStorageService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Get uploads path from configuration, default to "wwwroot/uploads"
        _uploadsPath = _configuration["FileStorage:UploadsPath"] ?? "wwwroot/uploads";

        // Get base URL from configuration for generating file URLs
        _baseUrl = _configuration["FileStorage:BaseUrl"] ?? _configuration["AppSettings:ApiUrl"] ?? "http://localhost:5000";

        // Ensure uploads directory exists
        if (!Directory.Exists(_uploadsPath))
        {
            Directory.CreateDirectory(_uploadsPath);
            _logger.LogInformation("Created uploads directory: {UploadsPath}", _uploadsPath);
        }
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string subfolder)
    {
        if (fileStream == null || fileStream.Length == 0)
        {
            throw new ArgumentException("File stream cannot be null or empty", nameof(fileStream));
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name cannot be null or empty", nameof(fileName));
        }

        // Sanitize file name
        var sanitizedFileName = SanitizeFileName(fileName);

        // Generate unique file name to prevent collisions
        var uniqueFileName = $"{Guid.NewGuid():N}-{sanitizedFileName}";

        // Create date-based subfolder structure (YYYY-MM-DD)
        var dateFolder = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var targetFolder = Path.Combine(_uploadsPath, subfolder, dateFolder);

        // Ensure target directory exists
        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }

        // Full file path
        var filePath = Path.Combine(targetFolder, uniqueFileName);

        // Save file
        try
        {
            using var fileStreamOutput = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await fileStream.CopyToAsync(fileStreamOutput);

            // Return relative path (for database storage)
            var relativePath = Path.Combine(subfolder, dateFolder, uniqueFileName)
                .Replace("\\", "/"); // Normalize to forward slashes

            _logger.LogInformation(
                "File saved successfully: {RelativePath} (Original: {FileName})",
                relativePath, fileName);

            return relativePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save file: {FileName}", fileName);
            throw;
        }
    }

    public async Task DeleteFileAsync(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new ArgumentException("Relative path cannot be null or empty", nameof(relativePath));
        }

        var filePath = GetFullPath(relativePath);

        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Attempted to delete non-existent file: {RelativePath}", relativePath);
            return;
        }

        try
        {
            await Task.Run(() => File.Delete(filePath));
            _logger.LogInformation("File deleted: {RelativePath}", relativePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file: {RelativePath}", relativePath);
            throw;
        }
    }

    public async Task<Stream?> GetFileAsync(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new ArgumentException("Relative path cannot be null or empty", nameof(relativePath));
        }

        var filePath = GetFullPath(relativePath);

        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Attempted to get non-existent file: {RelativePath}", relativePath);
            return null;
        }

        try
        {
            var memoryStream = new MemoryStream();
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            await fileStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            _logger.LogDebug("File retrieved: {RelativePath}", relativePath);
            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get file: {RelativePath}", relativePath);
            throw;
        }
    }

    public string GetFileUrl(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new ArgumentException("Relative path cannot be null or empty", nameof(relativePath));
        }

        // Normalize path separators
        var normalizedPath = relativePath.Replace("\\", "/");

        // Construct absolute URL
        var url = $"{_baseUrl.TrimEnd('/')}/uploads/{normalizedPath}";

        return url;
    }

    public async Task<(bool IsValid, string? ErrorMessage)> ValidateFileAsync(
        Stream stream,
        string fileName,
        string[] allowedExtensions,
        long maxSizeBytes)
    {
        // Check if stream is valid
        if (stream == null || stream.Length == 0)
        {
            return (false, "File is empty or could not be read");
        }

        // Check file size
        if (stream.Length > maxSizeBytes)
        {
            var maxSizeMB = maxSizeBytes / 1024.0 / 1024.0;
            var actualSizeMB = stream.Length / 1024.0 / 1024.0;
            return (false, $"File size ({actualSizeMB:F2} MB) exceeds maximum allowed size ({maxSizeMB:F2} MB)");
        }

        // Check file extension
        var fileExtension = Path.GetExtension(fileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
        {
            return (false, $"File type '{fileExtension}' is not allowed. Allowed types: {string.Join(", ", allowedExtensions)}");
        }

        // Additional validation: check file signature (magic bytes) for common types
        var isValidSignature = await ValidateFileSignatureAsync(stream, fileExtension);
        if (!isValidSignature)
        {
            return (false, $"File signature does not match extension '{fileExtension}'. File may be corrupted or misnamed.");
        }

        _logger.LogDebug(
            "File validation passed: {FileName}, Size: {SizeKB} KB, Extension: {Extension}",
            fileName, stream.Length / 1024, fileExtension);

        return (true, null);
    }

    public async Task<bool> FileExistsAsync(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return false;
        }

        var filePath = GetFullPath(relativePath);
        return await Task.FromResult(File.Exists(filePath));
    }

    public string GetFullPath(string relativePath)
    {
        // Normalize path separators
        var normalizedPath = relativePath.Replace("/", Path.DirectorySeparatorChar.ToString());
        return Path.Combine(_uploadsPath, normalizedPath);
    }

    #region Private Helper Methods

    /// <summary>
    /// Sanitizes file name by removing invalid characters and limiting length.
    /// </summary>
    private static string SanitizeFileName(string fileName)
    {
        // Remove invalid characters
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

        // Limit length (keep extension intact)
        var extension = Path.GetExtension(sanitized);
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(sanitized);

        const int maxNameLength = 100;
        if (nameWithoutExtension.Length > maxNameLength)
        {
            nameWithoutExtension = nameWithoutExtension.Substring(0, maxNameLength);
        }

        return nameWithoutExtension + extension;
    }

    /// <summary>
    /// Validates file signature (magic bytes) to ensure file type matches extension.
    /// Prevents users from renaming malicious files to bypass extension checks.
    /// </summary>
    private async Task<bool> ValidateFileSignatureAsync(Stream stream, string extension)
    {
        // Read first few bytes for signature check
        var buffer = new byte[8];
        var originalPosition = stream.Position;

        try
        {
            stream.Position = 0;
            await stream.ReadAsync(buffer, 0, buffer.Length);
            stream.Position = originalPosition; // Reset position

            // Check magic bytes for common file types
            return extension switch
            {
                ".pdf" => buffer[0] == 0x25 && buffer[1] == 0x50 && buffer[2] == 0x44 && buffer[3] == 0x46, // %PDF
                ".jpg" or ".jpeg" => buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF,
                ".png" => buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47,
                ".gif" => buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46,
                ".zip" => buffer[0] == 0x50 && buffer[1] == 0x4B,
                ".doc" => buffer[0] == 0xD0 && buffer[1] == 0xCF && buffer[2] == 0x11 && buffer[3] == 0xE0, // MS Compound Binary
                ".docx" or ".xlsx" => buffer[0] == 0x50 && buffer[1] == 0x4B, // Office Open XML (ZIP-based)
                ".txt" or ".csv" or ".json" or ".xml" => true, // Text files - skip signature check
                _ => true // Unknown type - allow (will be caught by extension check)
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to validate file signature for extension: {Extension}", extension);
            return true; // Allow if signature check fails (avoid false positives)
        }
    }

    #endregion
}
