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
        var basePath = settings.GetValueOrDefault("Storage:BasePath", "C:\\PortalForge\\Storage");
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
        return Path.Combine(categoryPath, uniqueFileName).Replace("\\", "/");
    }

    public async Task DeleteFileAsync(string relativePath)
    {
        var settings = await GetStorageSettingsAsync();
        var basePath = settings.GetValueOrDefault("Storage:BasePath", "C:\\PortalForge\\Storage");

        var fullPath = Path.Combine(basePath, relativePath.Replace("/", "\\"));

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
        var basePath = settings.GetValueOrDefault("Storage:BasePath", "C:\\PortalForge\\Storage");

        var fullPath = Path.Combine(basePath, relativePath.Replace("/", "\\"));
        return File.Exists(fullPath);
    }

    public string GetFullPath(string relativePath)
    {
        // This method is synchronous for performance in controllers
        // Use cached settings if available
        var basePath = _cachedSettings?.GetValueOrDefault("Storage:BasePath", "C:\\PortalForge\\Storage") 
                       ?? "C:\\PortalForge\\Storage";

        return Path.Combine(basePath, relativePath.Replace("/", "\\"));
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
}

