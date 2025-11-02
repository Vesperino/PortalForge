namespace PortalForge.Application.Interfaces;

public interface IStorageTestingService
{
    Task<StorageTestResult> TestStoragePathsAsync(Dictionary<string, string> settings);
}

public class StorageTestResult
{
    public string BasePath { get; set; } = string.Empty;
    public bool BasePathExists { get; set; }
    public bool BasePathWritable { get; set; }
    public List<SubdirectoryTestResult> Subdirectories { get; set; } = new();
    public bool Success { get; set; }
    public string? Message { get; set; }
}

public class SubdirectoryTestResult
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public bool Exists { get; set; }
    public string? Error { get; set; }
}
