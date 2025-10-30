using MediatR;

namespace PortalForge.Application.UseCases.Notifications.Commands.MarkAllAsRead;

public class MarkAllAsReadCommand : IRequest<Unit>
{
    public Guid UserId { get; set; }
}

