using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.Services;

/// <summary>
/// Service for creating and managing user notifications.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Create a notification for a user.
    /// </summary>
    Task CreateNotificationAsync(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        string? relatedEntityType = null,
        string? relatedEntityId = null,
        string? actionUrl = null
    );

    /// <summary>
    /// Notify an approver that a request is waiting for their approval.
    /// </summary>
    Task NotifyApproverAsync(Guid approverId, Request request);

    /// <summary>
    /// Notify the submitter about their request status.
    /// </summary>
    Task NotifySubmitterAsync(Request request, string message, NotificationType type);

    /// <summary>
    /// Get user's notifications (paginated).
    /// </summary>
    Task<List<Notification>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false, int pageNumber = 1, int pageSize = 20);

    /// <summary>
    /// Get count of unread notifications for a user.
    /// </summary>
    Task<int> GetUnreadCountAsync(Guid userId);

    /// <summary>
    /// Mark a notification as read.
    /// </summary>
    Task MarkAsReadAsync(Guid notificationId);

    /// <summary>
    /// Mark all notifications as read for a user.
    /// </summary>
    Task MarkAllAsReadAsync(Guid userId);
}


