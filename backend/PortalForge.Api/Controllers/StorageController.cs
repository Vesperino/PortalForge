using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StorageController : ControllerBase
{
    private readonly ILogger<StorageController> _logger;
    private readonly IFileStorageService _fileStorageService;

    public StorageController(
        ILogger<StorageController> logger,
        IFileStorageService fileStorageService)
    {
        _logger = logger;
        _fileStorageService = fileStorageService;
    }

    [HttpPost("upload/news-image")]
    public async Task<ActionResult<UploadImageResponse>> UploadNewsImage(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file provided" });
            }

            // Get storage settings for max file size
            var settings = await _fileStorageService.GetStorageSettingsAsync();
            var maxFileSizeMB = int.Parse(settings.GetValueOrDefault("Storage:MaxFileSizeMB", "10"));
            var maxFileSize = maxFileSizeMB * 1024 * 1024;

            if (file.Length > maxFileSize)
            {
                return BadRequest(new { message = $"File size exceeds {maxFileSizeMB}MB limit" });
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new { message = "Invalid file type. Allowed: JPG, PNG, GIF, WebP" });
            }

            _logger.LogInformation("Uploading file: {FileName}", file.FileName);

            // Save file using storage service
            using var fileStream = file.OpenReadStream();
            var relativePath = await _fileStorageService.SaveFileAsync(fileStream, file.FileName, "news-images");

            // Build URL for frontend to access the file
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var publicUrl = $"{baseUrl}/api/storage/files/{relativePath}";

            _logger.LogInformation("File uploaded successfully: {PublicUrl}", publicUrl);

            return Ok(new UploadImageResponse
            {
                Url = publicUrl,
                FileName = Path.GetFileName(relativePath),
                FilePath = relativePath
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return StatusCode(500, new { message = "Error uploading file", error = ex.Message });
        }
    }

    [HttpDelete("delete/news-image")]
    public async Task<ActionResult> DeleteNewsImage([FromQuery] string filePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return BadRequest(new { message = "File path is required" });
            }

            _logger.LogInformation("Deleting file: {FilePath}", filePath);

            await _fileStorageService.DeleteFileAsync(filePath);

            _logger.LogInformation("File deleted successfully: {FilePath}", filePath);

            return Ok(new { message = "File deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file");
            return StatusCode(500, new { message = "Error deleting file", error = ex.Message });
        }
    }

    /// <summary>
    /// Serves files from local storage with authorization
    /// </summary>
    [HttpGet("files/{category}/{fileName}")]
    [AllowAnonymous] // Allow anonymous for now - can be restricted based on category
    public async Task<IActionResult> GetFile(string category, string fileName)
    {
        try
        {
            // Validate inputs to prevent path traversal attacks
            if (string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest(new { message = "Invalid file path" });
            }

            if (category.Contains("..") || fileName.Contains("..") || 
                category.Contains("/") || category.Contains("\\") ||
                fileName.Contains("/") || fileName.Contains("\\"))
            {
                _logger.LogWarning("Path traversal attempt detected: {Category}/{FileName}", category, fileName);
                return BadRequest(new { message = "Invalid file path" });
            }

            var relativePath = $"{category}/{fileName}";
            
            // Check if file exists
            if (!await _fileStorageService.FileExistsAsync(relativePath))
            {
                return NotFound(new { message = "File not found" });
            }

            var fullPath = _fileStorageService.GetFullPath(relativePath);

            // Determine content type
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            // Return file with proper headers
            var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            
            return File(fileStream, contentType, fileName, enableRangeProcessing: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serving file: {Category}/{FileName}", category, fileName);
            return StatusCode(500, new { message = "Error serving file" });
        }
    }
}

public class UploadImageResponse
{
    public string Url { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
}

