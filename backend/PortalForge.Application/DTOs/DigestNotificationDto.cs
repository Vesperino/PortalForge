using PortalForge.Domain.Enums;

namespace PortalForge.Application.DTOs;

/// <summary>
/// Represents a digest notification containing multiple grouped notifications.
/// </summary>
public class DigestNotificationDto
{
    /// <summary>
    /// Type of digest (daily, weekly).
    /// </summary>
    public DigestType Type { get; set; }
    
    /// <summary>
    /// Period covered by this digest.
    /// </summary>
    public DateTime PeriodStart { get; set; }
    
    /// <summary>
    /// End of period covered by this digest.
    /// </summary>
    public DateTime PeriodEnd { get; set; }
    
    /// <summary>
    /// Total number of notifications in this digest.
    /// </summary>
    public int TotalNotifications { get; set; }
    
    /// <summary>
    /// Grouped notifications in this digest.
    /// </summary>
    public List<NotificationGroupDto> Groups { get; set; } = new();
    
    /// <summary>
    /// Summary message for the digest.
    /// </summary>
    public string Summary { get; set; } = string.Empty;
}