namespace PortalForge.Domain.Entities;

/// <summary>
/// Represents a system-wide configuration setting.
/// </summary>
public class SystemSetting
{
    public int Id { get; set; }
    
    /// <summary>
    /// Unique key for the setting (e.g., "Storage:BasePath")
    /// </summary>
    public string Key { get; set; } = string.Empty;
    
    /// <summary>
    /// Value of the setting
    /// </summary>
    public string Value { get; set; } = string.Empty;
    
    /// <summary>
    /// Category for grouping settings (e.g., "Storage", "Email", "Security")
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of what this setting does
    /// </summary>
    public string? Description { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public Guid? UpdatedBy { get; set; }
    public User? UpdatedByUser { get; set; }
}


