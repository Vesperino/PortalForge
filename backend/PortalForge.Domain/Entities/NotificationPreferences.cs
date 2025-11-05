using PortalForge.Domain.Enums;

namespace PortalForge.Domain.Entities;

/// <summary>
/// User preferences for notifications.
/// </summary>
public class NotificationPreferences
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// The user these preferences belong to.
    /// </summary>
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    /// <summary>
    /// Whether email notifications are enabled.
    /// </summary>
    public bool EmailEnabled { get; set; } = true;
    
    /// <summary>
    /// Whether in-app notifications are enabled.
    /// </summary>
    public bool InAppEnabled { get; set; } = true;
    
    /// <summary>
    /// Whether digest notifications are enabled.
    /// </summary>
    public bool DigestEnabled { get; set; } = false;
    
    /// <summary>
    /// Frequency of digest notifications.
    /// </summary>
    public DigestFrequency DigestFrequency { get; set; } = DigestFrequency.Daily;
    
    /// <summary>
    /// JSON array of disabled notification types.
    /// </summary>
    public string DisabledTypes { get; set; } = "[]";
    
    /// <summary>
    /// Whether to group similar notifications together.
    /// </summary>
    public bool GroupSimilarNotifications { get; set; } = true;
    
    /// <summary>
    /// Maximum number of notifications to group together.
    /// </summary>
    public int MaxGroupSize { get; set; } = 5;
    
    /// <summary>
    /// Time window in minutes for grouping notifications.
    /// </summary>
    public int GroupingTimeWindowMinutes { get; set; } = 60;
    
    /// <summary>
    /// Whether to send real-time notifications.
    /// </summary>
    public bool RealTimeEnabled { get; set; } = true;
    
    /// <summary>
    /// When these preferences were created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// When these preferences were last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}