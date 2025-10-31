using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalForge.Infrastructure.Persistence;

namespace PortalForge.Api.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class SystemSettingsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SystemSettingsController> _logger;

    public SystemSettingsController(
        ApplicationDbContext context,
        ILogger<SystemSettingsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all system settings
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<SystemSettingDto>>> GetAll()
    {
        var settings = await _context.SystemSettings
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Key)
            .Select(s => new SystemSettingDto
            {
                Id = s.Id,
                Key = s.Key,
                Value = s.Value,
                Category = s.Category,
                Description = s.Description,
                UpdatedAt = s.UpdatedAt
            })
            .ToListAsync();

        return Ok(settings);
    }

    /// <summary>
    /// Get a specific system setting by key
    /// </summary>
    [HttpGet("{key}")]
    public async Task<ActionResult<SystemSettingDto>> GetByKey(string key)
    {
        var setting = await _context.SystemSettings
            .Where(s => s.Key == key)
            .Select(s => new SystemSettingDto
            {
                Id = s.Id,
                Key = s.Key,
                Value = s.Value,
                Category = s.Category,
                Description = s.Description,
                UpdatedAt = s.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (setting == null)
        {
            return NotFound(new { message = $"Setting with key '{key}' not found" });
        }

        return Ok(setting);
    }

    /// <summary>
    /// Update system settings (batch update)
    /// </summary>
    [HttpPut]
    public async Task<ActionResult> UpdateSettings([FromBody] List<UpdateSettingRequest> updates)
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = userIdClaim != null ? Guid.Parse(userIdClaim) : null;

            foreach (var update in updates)
            {
                var setting = await _context.SystemSettings
                    .FirstOrDefaultAsync(s => s.Key == update.Key);

                if (setting != null)
                {
                    setting.Value = update.Value;
                    setting.UpdatedAt = DateTime.UtcNow;
                    setting.UpdatedBy = userId;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("System settings updated successfully by user {UserId}", userId);

            return Ok(new { message = "Settings updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating system settings");
            return StatusCode(500, new { message = "Error updating settings", error = ex.Message });
        }
    }

    /// <summary>
    /// Test if storage paths are accessible
    /// </summary>
    [HttpPost("test-storage")]
    public async Task<ActionResult<StorageTestResult>> TestStorage()
    {
        try
        {
            var settings = await _context.SystemSettings
                .Where(s => s.Category == "Storage")
                .ToDictionaryAsync(s => s.Key, s => s.Value);

            var basePath = settings.GetValueOrDefault("Storage:BasePath", "C:\\PortalForge\\Storage");
            var newsImagesPath = settings.GetValueOrDefault("Storage:NewsImagesPath", "news-images");
            var documentsPath = settings.GetValueOrDefault("Storage:DocumentsPath", "documents");

            var results = new StorageTestResult
            {
                BasePath = basePath,
                BasePathExists = Directory.Exists(basePath),
                BasePathWritable = false,
                Subdirectories = new List<SubdirectoryTest>()
            };

            // Test base path writability
            if (results.BasePathExists)
            {
                try
                {
                    var testFile = Path.Combine(basePath, $"test_{Guid.NewGuid()}.tmp");
                    await System.IO.File.WriteAllTextAsync(testFile, "test");
                    System.IO.File.Delete(testFile);
                    results.BasePathWritable = true;
                }
                catch
                {
                    results.BasePathWritable = false;
                }
            }
            else
            {
                // Try to create base path
                try
                {
                    Directory.CreateDirectory(basePath);
                    results.BasePathExists = true;
                    results.BasePathWritable = true;
                }
                catch (Exception ex)
                {
                    results.Message = $"Cannot create base path: {ex.Message}";
                }
            }

            // Test subdirectories
            var subdirs = new[] { 
                (newsImagesPath, "NewsImages"), 
                (documentsPath, "Documents") 
            };

            foreach (var (path, name) in subdirs)
            {
                var fullPath = Path.Combine(basePath, path);
                var subdirTest = new SubdirectoryTest
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
                    }
                    catch (Exception ex)
                    {
                        subdirTest.Error = ex.Message;
                    }
                }

                results.Subdirectories.Add(subdirTest);
            }

            results.Success = results.BasePathExists && results.BasePathWritable && 
                            results.Subdirectories.All(s => s.Exists);

            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing storage");
            return StatusCode(500, new { message = "Error testing storage", error = ex.Message });
        }
    }
}

public class SystemSettingDto
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UpdateSettingRequest
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class StorageTestResult
{
    public string BasePath { get; set; } = string.Empty;
    public bool BasePathExists { get; set; }
    public bool BasePathWritable { get; set; }
    public List<SubdirectoryTest> Subdirectories { get; set; } = new();
    public bool Success { get; set; }
    public string? Message { get; set; }
}

public class SubdirectoryTest
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public bool Exists { get; set; }
    public string? Error { get; set; }
}




