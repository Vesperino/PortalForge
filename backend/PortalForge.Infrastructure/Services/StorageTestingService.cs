using Microsoft.Extensions.Logging;
using PortalForge.Application.Interfaces;

namespace PortalForge.Infrastructure.Services;

public class StorageTestingService : IStorageTestingService
{
    private readonly ILogger<StorageTestingService> _logger;

    public StorageTestingService(ILogger<StorageTestingService> logger)
    {
        _logger = logger;
    }

    public async Task<StorageTestResult> TestStoragePathsAsync(Dictionary<string, string> settings)
    {
        var basePath = settings.GetValueOrDefault("Storage:BasePath", "C:\\PortalForge\\Storage");
        var newsImagesPath = settings.GetValueOrDefault("Storage:NewsImagesPath", "news-images");
        var documentsPath = settings.GetValueOrDefault("Storage:DocumentsPath", "documents");

        var result = new StorageTestResult
        {
            BasePath = basePath,
            BasePathExists = Directory.Exists(basePath),
            BasePathWritable = false,
            Subdirectories = new List<SubdirectoryTestResult>()
        };

        // Test base path writability
        if (result.BasePathExists)
        {
            try
            {
                var testFile = Path.Combine(basePath, $"test_{Guid.NewGuid()}.tmp");
                await File.WriteAllTextAsync(testFile, "test");
                File.Delete(testFile);
                result.BasePathWritable = true;
                _logger.LogInformation("Base path {BasePath} is writable", basePath);
            }
            catch (Exception ex)
            {
                result.BasePathWritable = false;
                _logger.LogWarning(ex, "Base path {BasePath} is not writable", basePath);
            }
        }
        else
        {
            // Try to create base path
            try
            {
                Directory.CreateDirectory(basePath);
                result.BasePathExists = true;
                result.BasePathWritable = true;
                _logger.LogInformation("Created base path {BasePath}", basePath);
            }
            catch (Exception ex)
            {
                result.Message = $"Cannot create base path: {ex.Message}";
                _logger.LogError(ex, "Cannot create base path {BasePath}", basePath);
            }
        }

        // Test subdirectories
        var subdirs = new[]
        {
            (newsImagesPath, "NewsImages"),
            (documentsPath, "Documents")
        };

        foreach (var (path, name) in subdirs)
        {
            var fullPath = Path.Combine(basePath, path);
            var subdirTest = new SubdirectoryTestResult
            {
                Name = name,
                Path = path,
                FullPath = fullPath,
                Exists = Directory.Exists(fullPath)
            };

            if (!subdirTest.Exists)
            {
                try
                {
                    Directory.CreateDirectory(fullPath);
                    subdirTest.Exists = true;
                    _logger.LogInformation("Created subdirectory {FullPath}", fullPath);
                }
                catch (Exception ex)
                {
                    subdirTest.Error = ex.Message;
                    _logger.LogError(ex, "Cannot create subdirectory {FullPath}", fullPath);
                }
            }

            result.Subdirectories.Add(subdirTest);
        }

        result.Success = result.BasePathExists && result.BasePathWritable &&
                        result.Subdirectories.All(s => s.Exists);

        return result;
    }
}
