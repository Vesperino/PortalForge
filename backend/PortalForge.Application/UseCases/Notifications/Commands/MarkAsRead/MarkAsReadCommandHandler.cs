using MediatR;
using PortalForge.Application.Services;

namespace PortalForge.Application.UseCases.Notifications.Commands.MarkAsRead;

public class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand, Unit>
{
    private readonly INotificationService _notificationService;

    public MarkAsReadCommandHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<Unit> Handle(MarkAsReadCommand command, CancellationToken cancellationToken)
    {
        await _notificationService.MarkAsReadAsync(command.NotificationId);
        return Unit.Value;
    }
}

