using PortalForge.Application.DTOs;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.Services;

/// <summary>
/// Enhanced notification service with smart grouping, preferences, and real-time capabilities.
/// </summary>
public interface ISmartNotificationService : INotificationService
{
    /// <summary>
    /// Send grouped notifications to a user based on their preferences.
    /// </summary>
    /// <param name="userId">User to send notifications to.</param>
    /// <param name="groups">Groups of notifications to send.</param>
    Task SendGroupedNotificationsAsync(Guid userId, List<NotificationGroupDto> groups);
    
    /// <summary>
    /// Get user's notification preferences.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <returns>User's notification preferences or default if none exist.</returns>
    Task<NotificationPreferences> GetUserPreferencesAsync(Guid userId);
    
    /// <summary>
    /// Update user's notification preferences.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <param name="preferences">New preferences.</param>
    Task UpdateUserPreferencesAsync(Guid userId, NotificationPreferences preferences);
    
    /// <summary>
    /// Send digest notification to a user.
    /// </summary>
    /// <param name="userId">User to send digest to.</param>
    /// <param name="type">Type of digest (daily, weekly).</param>
    Task SendDigestNotificationAsync(Guid userId, DigestType type);
    
    /// <summary>
    /// Send real-time notification to a user if they have real-time notifications enabled.
    /// </summary>
    /// <param name="userId">User to notify.</param>
    /// <param name="message">Notification message.</param>
    /// <param name="type">Type of notification.</param>
    Task SendRealTimeNotificationAsync(Guid userId, string message, NotificationType type);
    
    /// <summary>
    /// Group similar notifications for a user based on their preferences.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <param name="notifications">Notifications to group.</param>
    /// <returns>Grouped notifications.</returns>
    Task<List<NotificationGroupDto>> GroupNotificationsAsync(Guid userId, List<Notification> notifications);
    
    /// <summary>
    /// Check if a notification type is enabled for a user.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <param name="type">Notification type to check.</param>
    /// <returns>True if the notification type is enabled.</returns>
    Task<bool> IsNotificationTypeEnabledAsync(Guid userId, NotificationType type);
    
    /// <summary>
    /// Generate digest for a user for a specific period.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <param name="periodStart">Start of period.</param>
    /// <param name="periodEnd">End of period.</param>
    /// <returns>Digest notification data.</returns>
    Task<DigestNotificationDto> GenerateDigestAsync(Guid userId, DateTime periodStart, DateTime periodEnd);
    
    /// <summary>
    /// Send notification with smart processing (grouping, preferences, etc.).
    /// </summary>
    /// <param name="userId">User to notify.</param>
    /// <param name="type">Type of notification.</param>
    /// <param name="title">Notification title.</param>
    /// <param name="message">Notification message.</param>
    /// <param name="relatedEntityType">Type of related entity.</param>
    /// <param name="relatedEntityId">ID of related entity.</param>
    /// <param name="actionUrl">URL for notification action.</param>
    Task CreateSmartNotificationAsync(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        string? relatedEntityType = null,
        string? relatedEntityId = null,
        string? actionUrl = null);
}