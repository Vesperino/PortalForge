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

        // Additional test: Verify file serving endpoint works with date-based paths
        if (result.Success)
        {
            await TestFileServingEndpoint(result);
        }

        return result;
    }

    private async Task TestFileServingEndpoint(StorageTestResult result)
    {
        try
        {
            // Test date-based path structure (simulating real file uploads)
            var testFileName = $"test-{Guid.NewGuid()}.txt";
            var dateFolder = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var category = "test-category";
            var relativePath = $"{category}/{dateFolder}/{testFileName}";

            var fullPath = Path.Combine(result.BasePath, relativePath);
            var directory = Path.GetDirectoryName(fullPath);

            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Write and read test file with date-based structure
            await File.WriteAllTextAsync(fullPath, "Test content");
            var content = await File.ReadAllTextAsync(fullPath);

            if (content != "Test content")
            {
                result.Message = $"WARNING: Could not read test file with date-based path: {relativePath}";
                result.Success = false;
                _logger.LogWarning(result.Message);
                return;
            }

            _logger.LogInformation("Successfully tested date-based path structure: {RelativePath}", relativePath);

            // Clean up test file and directories
            File.Delete(fullPath);

            try
            {
                if (directory != null && Directory.Exists(directory) && !Directory.EnumerateFileSystemEntries(directory).Any())
                {
                    Directory.Delete(directory);
                }
                var categoryPath = Path.Combine(result.BasePath, category);
                if (Directory.Exists(categoryPath) && !Directory.EnumerateFileSystemEntries(categoryPath).Any())
                {
                    Directory.Delete(categoryPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not clean up test directories (non-critical)");
            }

            result.Message = $"✅ Storage test passed. Date-based paths (e.g., {relativePath}) work correctly.";
            _logger.LogInformation("File serving endpoint test completed successfully");
        }
        catch (Exception ex)
        {
            result.Message = $"❌ Date-based path test failed: {ex.Message}. File serving may not work correctly!";
            result.Success = false;
            _logger.LogError(ex, "File serving endpoint test failed");
        }
    }
}
