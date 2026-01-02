using MediatR;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;
using PortalForge.Application.Services;

namespace PortalForge.Application.UseCases.Notifications.Commands.MarkAsRead;

public class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand, Unit>
{
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public MarkAsReadCommandHandler(
        INotificationService notificationService,
        IUnitOfWork unitOfWork)
    {
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(MarkAsReadCommand command, CancellationToken cancellationToken)
    {
        var notification = await _unitOfWork.NotificationRepository.GetByIdAsync(command.NotificationId);

        if (notification == null)
        {
            throw new NotFoundException("Powiadomienie nie zostalo znalezione");
        }

        if (notification.UserId != command.UserId)
        {
            throw new ForbiddenException("Nie masz uprawnien do tej powiadomienia");
        }

        await _notificationService.MarkAsReadAsync(command.NotificationId);
        return Unit.Value;
    }
}

