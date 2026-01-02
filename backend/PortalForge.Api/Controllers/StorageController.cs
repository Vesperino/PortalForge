using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using PortalForge.Api.DTOs.Responses.Storage;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.Storage.Commands.UploadNewsImage;
using PortalForge.Application.UseCases.Storage.Commands.DeleteNewsImage;
using PortalForge.Application.UseCases.Storage.Commands.UploadServiceIcon;
using PortalForge.Application.UseCases.Storage.Commands.UploadCommentAttachment;

namespace PortalForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StorageController : BaseController
{
    private readonly IMediator _mediator;
    private readonly ILogger<StorageController> _logger;
    private readonly IFileStorageService _fileStorageService;
    private readonly IFileValidationService _fileValidationService;

    public StorageController(
        IMediator mediator,
        ILogger<StorageController> logger,
        IFileStorageService fileStorageService,
        IFileValidationService fileValidationService)
    {
        _mediator = mediator;
        _logger = logger;
        _fileStorageService = fileStorageService;
        _fileValidationService = fileValidationService;
    }

    [HttpPost("upload/news-image")]
    [Authorize(Policy = "MarketingOrAdmin")]
    public async Task<ActionResult<UploadImageResponse>> UploadNewsImage(IFormFile file)
    {
        try
        {
            var validationResult = _fileValidationService.ValidateImageUpload(file?.FileName, file?.Length ?? 0);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { message = validationResult.ErrorMessage });
            }

            using var fileStream = file!.OpenReadStream();

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
            return StatusCode(500, new { message = "Error uploading file" });
        }
    }

    [HttpPost("upload/service-icon")]
    [Authorize(Policy = "RequirePermission:internal_services.manage")]
    public async Task<ActionResult<UploadImageResponse>> UploadServiceIcon(IFormFile file)
    {
        try
        {
            var validationResult = _fileValidationService.ValidateImageUpload(file?.FileName, file?.Length ?? 0);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { message = validationResult.ErrorMessage });
            }

            using var fileStream = file!.OpenReadStream();

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
            return StatusCode(500, new { message = "Error uploading service icon" });
        }
    }

    [HttpPost("upload/comment-attachment")]
    [Authorize]
    public async Task<ActionResult<UploadImageResponse>> UploadCommentAttachment(IFormFile file)
    {
        try
        {
            var validationResult = _fileValidationService.ValidateAttachmentUpload(file?.FileName, file?.Length ?? 0);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { message = validationResult.ErrorMessage });
            }

            using var fileStream = file!.OpenReadStream();

            var command = new UploadCommentAttachmentCommand
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
            _logger.LogError(ex, "Error uploading comment attachment");
            return StatusCode(500, new { message = "Error uploading comment attachment" });
        }
    }

    [HttpDelete("delete/news-image")]
    [Authorize(Policy = "MarketingOrAdmin")]
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
            return StatusCode(500, new { message = "Error deleting file" });
        }
    }

    /// <summary>
    /// Serves files from local storage.
    /// Public paths (images, news-images, service-icons) are accessible without authentication.
    /// Private paths (comment-attachments, request-attachments) require authentication.
    /// </summary>
    [HttpGet("files/{**relativePath}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFile(string relativePath)
    {
        try
        {
            // Validate inputs to prevent path traversal attacks
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return BadRequest(new { message = "Invalid file path" });
            }

            // Decode URL to catch encoded path traversal attempts (including double encoding)
            var decodedPath = Uri.UnescapeDataString(relativePath);
            // Decode again to catch double-encoded attempts like %252e%252e
            var doubleDecodedPath = Uri.UnescapeDataString(decodedPath);

            // Check for path traversal patterns in both decoded versions
            if (ContainsPathTraversalPattern(decodedPath) || ContainsPathTraversalPattern(doubleDecodedPath))
            {
                _logger.LogWarning("Path traversal attempt detected: {RelativePath}", relativePath);
                return BadRequest(new { message = "Invalid file path" });
            }

            // Use the decoded path for further processing
            relativePath = decodedPath;

            // Normalize path separators
            relativePath = relativePath.Replace("\\", "/");

            // Additional security: Validate resolved path is within allowed directory
            var basePath = _fileStorageService.GetBasePath();
            var resolvedPath = Path.GetFullPath(Path.Combine(basePath, relativePath));
            if (!resolvedPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Path traversal attempt via path resolution: {RelativePath} resolved to {ResolvedPath}", relativePath, resolvedPath);
                return BadRequest(new { message = "Invalid file path" });
            }

            // Check if path requires authentication (private files)
            var privatePaths = new[] { "comment-attachments", "request-attachments", "sick-leaves", "documents" };
            var isPrivatePath = privatePaths.Any(p => relativePath.StartsWith(p, StringComparison.OrdinalIgnoreCase));

            if (isPrivatePath && !User.Identity?.IsAuthenticated == true)
            {
                return Unauthorized(new { message = "Authentication required for this resource" });
            }

            // Check if file exists
            if (!await _fileStorageService.FileExistsAsync(relativePath))
            {
                _logger.LogWarning("File not found: {RelativePath}", relativePath);
                return NotFound(new { message = "File not found" });
            }

            var fullPath = _fileStorageService.GetFullPath(relativePath);

            // Extract filename for content type detection
            var fileName = Path.GetFileName(fullPath);

            // Determine content type
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            // Return file with proper headers
            var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);

            // Define content types that should be displayed inline in the browser
            var inlineContentTypes = new[]
            {
                "application/pdf",
                "text/plain",
                "text/html",
                "text/css",
                "text/javascript",
                "image/jpeg",
                "image/png",
                "image/gif",
                "image/svg+xml",
                "image/webp",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document", // .docx
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", // .xlsx
                "application/msword", // .doc
                "application/vnd.ms-excel" // .xls
            };

            // Return file with inline disposition for supported types, download for others
            if (inlineContentTypes.Contains(contentType))
            {
                // For inline viewing, don't specify fileName to avoid Content-Disposition: attachment
                return File(fileStream, contentType, enableRangeProcessing: true);
            }
            else
            {
                // For other file types, trigger download with original filename
                return File(fileStream, contentType, fileName, enableRangeProcessing: true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serving file: {RelativePath}", relativePath);
            return StatusCode(500, new { message = "Error serving file" });
        }
    }

    /// <summary>
    /// Checks if a path contains any path traversal patterns.
    /// </summary>
    private static bool ContainsPathTraversalPattern(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        // Check for common path traversal patterns
        var dangerousPatterns = new[]
        {
            "..",           // Parent directory
            "..\\",         // Windows parent
            "../",          // Unix parent
            "\\",           // Backslash (normalize to forward slash)
            "%",            // URL encoding remnants after decode
            "\0",           // Null byte injection
            "..%",          // Partial encoded traversal
            "%2e",          // Encoded dot
            "%2f",          // Encoded forward slash
            "%5c",          // Encoded backslash
        };

        var lowerPath = path.ToLowerInvariant();
        return dangerousPatterns.Any(pattern => lowerPath.Contains(pattern.ToLowerInvariant()));
    }
}

