using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Infrastructure.Services;

/// <summary>
/// Adapter to make Application.Interfaces.IFileStorageService compatible with Application.Common.Interfaces.IFileStorageService
/// This is a temporary solution until interfaces are unified
/// </summary>
public class FileStorageServiceAdapter : IFileStorageService
{
    private readonly PortalForge.Application.Interfaces.IFileStorageService _fileStorageService;

    public FileStorageServiceAdapter(PortalForge.Application.Interfaces.IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
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
        // FileStorageService doesn't have GetFullPath, return URL instead as fallback
        return _fileStorageService.GetFileUrl(relativePath);
    }

    public Task<Dictionary<string, string>> GetStorageSettingsAsync()
    {
        // Return empty dictionary as FileStorageService doesn't have this method
        return Task.FromResult(new Dictionary<string, string>());
    }
}
