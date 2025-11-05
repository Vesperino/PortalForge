using PortalForge.Domain.Enums;

namespace PortalForge.Application.DTOs;

/// <summary>
/// Represents a group of similar notifications.
/// </summary>
public class NotificationGroupDto
{
    /// <summary>
    /// Type of notifications in this group.
    /// </summary>
    public NotificationType Type { get; set; }
    
    /// <summary>
    /// Number of notifications in this group.
    /// </summary>
    public int Count { get; set; }
    
    /// <summary>
    /// Title for the grouped notification.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Summary message for the group.
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// URL to view all notifications in this group.
    /// </summary>
    public string? ActionUrl { get; set; }
    
    /// <summary>
    /// When the first notification in this group was created.
    /// </summary>
    public DateTime FirstCreatedAt { get; set; }
    
    /// <summary>
    /// When the last notification in this group was created.
    /// </summary>
    public DateTime LastCreatedAt { get; set; }
    
    /// <summary>
    /// Individual notification IDs in this group.
    /// </summary>
    public List<Guid> NotificationIds { get; set; } = new();
}