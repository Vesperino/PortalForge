using PortalForge.Domain.Entities;

namespace PortalForge.Application.Interfaces;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id);
    Task<List<Notification>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false, int pageNumber = 1, int pageSize = 20);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task CreateAsync(Notification notification);
    Task UpdateAsync(Notification notification);
    Task MarkAsReadAsync(Guid notificationId);
    Task MarkAllAsReadAsync(Guid userId);
    Task DeleteAsync(Guid id);
}


