using PortalForge.Application.UseCases.Notifications.DTOs;

namespace PortalForge.Application.UseCases.Notifications.Queries.GetUserNotifications;

public class GetUserNotificationsResult
{
    public List<NotificationDto> Notifications { get; set; } = new();
}

