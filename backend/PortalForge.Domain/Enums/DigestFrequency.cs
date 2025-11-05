namespace PortalForge.Domain.Enums;

/// <summary>
/// Frequency options for digest notifications.
/// </summary>
public enum DigestFrequency
{
    /// <summary>
    /// Daily digest notifications.
    /// </summary>
    Daily,
    
    /// <summary>
    /// Weekly digest notifications.
    /// </summary>
    Weekly,
    
    /// <summary>
    /// Never send digest notifications.
    /// </summary>
    Never
}