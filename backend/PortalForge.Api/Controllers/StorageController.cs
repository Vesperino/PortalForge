using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PortalForge.Infrastructure.Auth;
using Supabase;

namespace PortalForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StorageController : ControllerBase
{
    private readonly ILogger<StorageController> _logger;
    private readonly Client _supabaseClient;

    public StorageController(
        ILogger<StorageController> logger,
        IOptions<SupabaseSettings> supabaseSettings)
    {
        _logger = logger;

        // Initialize Supabase client with service role key (bypasses RLS)
        var settings = supabaseSettings.Value;
        var options = new SupabaseOptions
        {
            AutoRefreshToken = false,
            AutoConnectRealtime = false
        };

        _supabaseClient = new Client(settings.Url, settings.ServiceRoleKey, options);
        _supabaseClient.InitializeAsync().Wait();
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

            // Validate file size (max 5MB)
            const long maxFileSize = 5 * 1024 * 1024;
            if (file.Length > maxFileSize)
            {
                return BadRequest(new { message = "File size exceeds 5MB limit" });
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new { message = "Invalid file type. Allowed: JPG, PNG, GIF, WebP" });
            }

            // Generate unique filename
            var fileName = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}-{Guid.NewGuid():N}{fileExtension}";
            var filePath = $"news-images/{fileName}";

            _logger.LogInformation("Uploading file: {FileName} to {FilePath}", file.FileName, filePath);

            // Read file content
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();

            // Upload to Supabase Storage
            var uploadResult = await _supabaseClient.Storage
                .From("news-images")
                .Upload(fileBytes, filePath, new Supabase.Storage.FileOptions
                {
                    CacheControl = "3600",
                    Upsert = false
                });

            if (uploadResult == null)
            {
                _logger.LogError("Upload failed: No result returned from Supabase");
                return StatusCode(500, new { message = "Upload failed" });
            }

            // Get public URL
            var publicUrl = _supabaseClient.Storage
                .From("news-images")
                .GetPublicUrl(filePath);

            _logger.LogInformation("File uploaded successfully: {PublicUrl}", publicUrl);

            return Ok(new UploadImageResponse
            {
                Url = publicUrl,
                FileName = fileName,
                FilePath = filePath
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

            // Delete from Supabase Storage
            await _supabaseClient.Storage
                .From("news-images")
                .Remove(new List<string> { filePath });

            _logger.LogInformation("File deleted successfully: {FilePath}", filePath);

            return Ok(new { message = "File deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file");
            return StatusCode(500, new { message = "Error deleting file", error = ex.Message });
        }
    }
}

public class UploadImageResponse
{
    public string Url { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
}

