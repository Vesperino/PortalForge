using MediatR;

namespace PortalForge.Application.UseCases.Notifications.Commands.MarkAsRead;

public class MarkAsReadCommand : IRequest<Unit>
{
    public Guid NotificationId { get; set; }
}

