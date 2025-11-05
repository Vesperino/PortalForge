using Microsoft.Extensions.Configuration;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Infrastructure.Services;

/// <summary>
/// Adapter to make Application.Interfaces.IFileStorageService compatible with Application.Common.Interfaces.IFileStorageService
/// This is a temporary solution until interfaces are unified
/// </summary>
public class FileStorageServiceAdapter : IFileStorageService
{
    private readonly PortalForge.Application.Interfaces.IFileStorageService _fileStorageService;
    private readonly IConfiguration _configuration;
    private readonly string _uploadsPath;

    public FileStorageServiceAdapter(
        PortalForge.Application.Interfaces.IFileStorageService fileStorageService,
        IConfiguration configuration)
    {
        _fileStorageService = fileStorageService;
        _configuration = configuration;

        // Get uploads path from configuration (same as FileStorageService)
        _uploadsPath = _configuration["FileStorage:UploadsPath"] ?? "wwwroot/uploads";
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string category)
    {
        return await _fileStorageService.SaveFileAsync(fileStream, fileName, category);
    }

    public async Task DeleteFileAsync(string relativePath)
    {
        await _fileStorageService.DeleteFileAsync(relativePath);
    }

    public async Task<bool> FileExistsAsync(string relativePath)
    {
        return await _fileStorageService.FileExistsAsync(relativePath);
    }

    public string GetFullPath(string relativePath)
    {
        // Build full file system path (same logic as FileStorageService)
        var normalizedPath = relativePath.Replace("/", Path.DirectorySeparatorChar.ToString());
        return Path.Combine(_uploadsPath, normalizedPath);
    }

    public Task<Dictionary<string, string>> GetStorageSettingsAsync()
    {
        // Return empty dictionary as FileStorageService doesn't have this method
        return Task.FromResult(new Dictionary<string, string>());
    }
}
