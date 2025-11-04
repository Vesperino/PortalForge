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

    /// <summary>
    /// Notify a substitute that they will be covering for someone on vacation.
    /// </summary>
    Task NotifySubstituteAsync(Guid substituteId, VacationSchedule schedule);

    /// <summary>
    /// Notify a substitute that a vacation they're covering has started.
    /// </summary>
    Task NotifyVacationStartedAsync(Guid substituteId, VacationSchedule schedule);

    /// <summary>
    /// Notify a user that their vacation has ended.
    /// </summary>
    Task NotifyVacationEndedAsync(Guid userId, VacationSchedule schedule);

    /// <summary>
    /// Send vacation-related notification (cancellation, approval, rejection).
    /// </summary>
    /// <param name="userId">User to notify.</param>
    /// <param name="type">Type of vacation notification.</param>
    /// <param name="request">The vacation request.</param>
    Task SendVacationNotificationAsync(Guid userId, NotificationType type, Request request);

    /// <summary>
    /// Send SLA reminder to approver about overdue request.
    /// </summary>
    /// <param name="approverId">Approver to notify.</param>
    /// <param name="request">The overdue request.</param>
    /// <param name="daysOverdue">Number of days past the SLA threshold.</param>
    Task SendSLAReminderAsync(Guid approverId, Request request, int daysOverdue);

    /// <summary>
    /// Notify submitter that their request requires completion/additional information.
    /// </summary>
    /// <param name="userId">Submitter to notify.</param>
    /// <param name="request">The request requiring completion.</param>
    /// <param name="reason">Reason why completion is required.</param>
    Task SendRequestCompletionRequiredAsync(Guid userId, Request request, string reason);

    /// <summary>
    /// Warn user about expiring carried-over vacation days.
    /// </summary>
    /// <param name="userId">User to notify.</param>
    /// <param name="expiryDate">When the vacation days expire.</param>
    /// <param name="daysRemaining">Number of carried-over days that will expire.</param>
    Task SendVacationExpiryWarningAsync(Guid userId, DateTime expiryDate, int daysRemaining);

    /// <summary>
    /// Notify supervisor about submitted sick leave (L4) - for information only.
    /// </summary>
    /// <param name="supervisorId">Supervisor to notify.</param>
    /// <param name="sickLeave">The sick leave that was submitted.</param>
    Task SendSickLeaveNotificationAsync(Guid supervisorId, SickLeave sickLeave);
}


