using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using PortalForge.Api.DTOs.Responses.Storage;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.Storage.Commands.UploadNewsImage;
using PortalForge.Application.UseCases.Storage.Commands.DeleteNewsImage;
using PortalForge.Application.UseCases.Storage.Commands.UploadServiceIcon;

namespace PortalForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StorageController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<StorageController> _logger;
    private readonly IFileStorageService _fileStorageService;

    public StorageController(
        IMediator mediator,
        ILogger<StorageController> logger,
        IFileStorageService fileStorageService)
    {
        _mediator = mediator;
        _logger = logger;
        _fileStorageService = fileStorageService;
    }

    [HttpPost("upload/news-image")]
    public async Task<ActionResult<UploadImageResponse>> UploadNewsImage(IFormFile file)
    {
        try
        {
            using var fileStream = file.OpenReadStream();

            var command = new UploadNewsImageCommand
            {
                FileStream = fileStream,
                FileName = file.FileName,
                FileSize = file.Length
            };

            var result = await _mediator.Send(command);

            // Build full URL with proper scheme and host
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var publicUrl = $"{baseUrl}/api/storage/files/{result.FilePath}";

            return Ok(new UploadImageResponse
            {
                Url = publicUrl,
                FileName = result.FileName,
                FilePath = result.FilePath
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return StatusCode(500, new { message = "Error uploading file", error = ex.Message });
        }
    }

    [HttpPost("upload/service-icon")]
    [Authorize(Policy = "RequirePermission:internal_services.manage")]
    public async Task<ActionResult<UploadImageResponse>> UploadServiceIcon(IFormFile file)
    {
        try
        {
            using var fileStream = file.OpenReadStream();

            var command = new UploadServiceIconCommand
            {
                FileStream = fileStream,
                FileName = file.FileName,
                FileSize = file.Length
            };

            var result = await _mediator.Send(command);

            // Build full URL with proper scheme and host
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var publicUrl = $"{baseUrl}/api/storage/files/{result.FilePath}";

            return Ok(new UploadImageResponse
            {
                Url = publicUrl,
                FileName = result.FileName,
                FilePath = result.FilePath
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading service icon");
            return StatusCode(500, new { message = "Error uploading service icon", error = ex.Message });
        }
    }

    [HttpDelete("delete/news-image")]
    public async Task<ActionResult> DeleteNewsImage([FromQuery] string filePath)
    {
        try
        {
            var command = new DeleteNewsImageCommand
            {
                FilePath = filePath
            };

            await _mediator.Send(command);

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

