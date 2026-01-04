using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace PortalForge.Api.Controllers;

/// <summary>
/// Public endpoint for application version information
/// </summary>
[ApiController]
[Route("api/version")]
public class VersionController : ControllerBase
{
    /// <summary>
    /// Get current backend version
    /// </summary>
    [HttpGet]
    public ActionResult<VersionInfo> GetVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version?.ToString() ?? "1.0.0";
        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? version;

        return Ok(new VersionInfo
        {
            Version = informationalVersion,
            BuildDate = GetBuildDate(assembly),
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        });
    }

    private static string GetBuildDate(Assembly assembly)
    {
        try
        {
            var location = assembly.Location;
            if (!string.IsNullOrEmpty(location))
            {
                var buildDate = System.IO.File.GetLastWriteTime(location);
                return buildDate.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        catch
        {
            // Ignore errors getting build date
        }
        return DateTime.UtcNow.ToString("yyyy-MM-dd");
    }
}

public class VersionInfo
{
    public string Version { get; set; } = string.Empty;
    public string BuildDate { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
}
