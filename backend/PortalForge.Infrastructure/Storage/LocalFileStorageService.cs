using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Infrastructure.Persistence;

namespace PortalForge.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LocalFileStorageService> _logger;
    private Dictionary<string, string>? _cachedSettings;
    private DateTime _cacheTime;
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(5);

    public LocalFileStorageService(
        ApplicationDbContext context,
        ILogger<LocalFileStorageService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string category)
    {
        var settings = await GetStorageSettingsAsync();
        var basePath = settings.GetValueOrDefault("Storage:BasePath", GetDefaultBasePath());
        var categoryPath = settings.GetValueOrDefault($"Storage:{category}Path", category);

        // Create full directory path
        var fullDirPath = Path.Combine(basePath, categoryPath);
        Directory.CreateDirectory(fullDirPath);

        // Generate unique filename to prevent overwriting
        var fileExtension = Path.GetExtension(fileName);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var uniqueFileName = $"{fileNameWithoutExtension}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{fileExtension}";

        var fullFilePath = Path.Combine(fullDirPath, uniqueFileName);

        // Save file
        using (var fileStreamOutput = new FileStream(fullFilePath, FileMode.Create, FileAccess.Write))
        {
            await fileStream.CopyToAsync(fileStreamOutput);
        }

        _logger.LogInformation("File saved: {FilePath}", fullFilePath);

        // Return relative path
        var normalizedCategory = categoryPath.Replace("\\", "/").Trim('/');
        return $"{normalizedCategory}/{uniqueFileName}";
    }

    public async Task DeleteFileAsync(string relativePath)
    {
        var settings = await GetStorageSettingsAsync();
        var basePath = settings.GetValueOrDefault("Storage:BasePath", GetDefaultBasePath());

        var fullPath = CombineWithBasePath(basePath, relativePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("File deleted: {FilePath}", fullPath);
        }
        else
        {
            _logger.LogWarning("File not found for deletion: {FilePath}", fullPath);
        }
    }

    public async Task<bool> FileExistsAsync(string relativePath)
    {
        var settings = await GetStorageSettingsAsync();
        var basePath = settings.GetValueOrDefault("Storage:BasePath", GetDefaultBasePath());

        var fullPath = CombineWithBasePath(basePath, relativePath);
        return File.Exists(fullPath);
    }

    public async Task<Stream?> GetFileAsync(string relativePath)
    {
        var settings = await GetStorageSettingsAsync();
        var basePath = settings.GetValueOrDefault("Storage:BasePath", GetDefaultBasePath());

        var fullPath = CombineWithBasePath(basePath, relativePath);

        if (!File.Exists(fullPath))
        {
            return null;
        }

        return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    public string GetFileUrl(string relativePath)
    {
        // Return URL for accessing the file through the API
        var normalizedPath = relativePath.Replace("\\", "/");
        return $"/api/storage/files/{normalizedPath}";
    }

    public Task<(bool IsValid, string? ErrorMessage)> ValidateFileAsync(
        Stream stream,
        string fileName,
        string[] allowedExtensions,
        long maxSizeBytes)
    {
        // Check file size
        if (stream.Length > maxSizeBytes)
        {
            return Task.FromResult<(bool, string?)>((false, $"File size exceeds maximum allowed size of {maxSizeBytes / 1024 / 1024}MB"));
        }

        // Check file extension
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            var allowed = string.Join(", ", allowedExtensions);
            return Task.FromResult<(bool, string?)>((false, $"File type not allowed. Allowed types: {allowed}"));
        }

        return Task.FromResult<(bool, string?)>((true, null));
    }

    public string GetFullPath(string relativePath)
    {
        // This method is synchronous for performance in controllers
        // Use cached settings if available
        var basePath = _cachedSettings?.GetValueOrDefault("Storage:BasePath", GetDefaultBasePath())
                       ?? GetDefaultBasePath();

        return CombineWithBasePath(basePath, relativePath);
    }

    public string GetBasePath()
    {
        var basePath = _cachedSettings?.GetValueOrDefault("Storage:BasePath", GetDefaultBasePath())
                       ?? GetDefaultBasePath();
        return Path.GetFullPath(basePath);
    }

    public async Task<Dictionary<string, string>> GetStorageSettingsAsync()
    {
        // Return cached settings if still valid
        if (_cachedSettings != null && DateTime.UtcNow - _cacheTime < _cacheExpiry)
        {
            return _cachedSettings;
        }

        // Fetch settings from database
        var settings = await _context.SystemSettings
            .Where(s => s.Category == "Storage")
            .ToDictionaryAsync(s => s.Key, s => s.Value);

        _cachedSettings = settings;
        _cacheTime = DateTime.UtcNow;

        return settings;
    }

    private static string GetDefaultBasePath()
    {
        return OperatingSystem.IsWindows() ? "C:\\PortalForge\\Storage" : "/app/storage";
    }

    private static string CombineWithBasePath(string basePath, string relativePath)
    {
        // Normalize relative path into segments and combine with base path using OS-specific separators
        var segments = relativePath
            .Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);

        var all = new string[segments.Length + 1];
        all[0] = basePath;
        Array.Copy(segments, 0, all, 1, segments.Length);

        return Path.Combine(all);
    }
}









