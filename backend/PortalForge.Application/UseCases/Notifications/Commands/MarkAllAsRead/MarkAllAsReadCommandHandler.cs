using MediatR;
using PortalForge.Application.Services;

namespace PortalForge.Application.UseCases.Notifications.Commands.MarkAllAsRead;

public class MarkAllAsReadCommandHandler : IRequestHandler<MarkAllAsReadCommand, Unit>
{
    private readonly INotificationService _notificationService;

    public MarkAllAsReadCommandHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<Unit> Handle(MarkAllAsReadCommand command, CancellationToken cancellationToken)
    {
        await _notificationService.MarkAllAsReadAsync(command.UserId);
        return Unit.Value;
    }
}

