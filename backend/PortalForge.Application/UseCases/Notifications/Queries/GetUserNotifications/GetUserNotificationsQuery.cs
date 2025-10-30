using MediatR;

namespace PortalForge.Application.UseCases.Notifications.Queries.GetUserNotifications;

public class GetUserNotificationsQuery : IRequest<GetUserNotificationsResult>
{
    public Guid UserId { get; set; }
    public bool UnreadOnly { get; set; } = false;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

