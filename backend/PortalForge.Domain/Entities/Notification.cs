using PortalForge.Domain.Enums;

namespace PortalForge.Domain.Entities;

/// <summary>
/// Represents an in-app notification for a user.
/// </summary>
public class Notification
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// The user who will receive this notification.
    /// </summary>
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    /// <summary>
    /// Type of notification (approval, rejection, etc.).
    /// </summary>
    public NotificationType Type { get; set; }
    
    /// <summary>
    /// Short title for the notification.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Detailed message content.
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of related entity (e.g., "Request", "News").
    /// </summary>
    public string? RelatedEntityType { get; set; }
    
    /// <summary>
    /// ID of the related entity.
    /// </summary>
    public string? RelatedEntityId { get; set; }
    
    /// <summary>
    /// URL to navigate to when notification is clicked.
    /// </summary>
    public string? ActionUrl { get; set; }
    
    /// <summary>
    /// Whether the notification has been read.
    /// </summary>
    public bool IsRead { get; set; } = false;
    
    /// <summary>
    /// When the notification was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// When the notification was marked as read.
    /// </summary>
    public DateTime? ReadAt { get; set; }
}


