using MediatR;
using PortalForge.Application.Services;
using PortalForge.Application.UseCases.Notifications.DTOs;

namespace PortalForge.Application.UseCases.Notifications.Queries.GetUserNotifications;

public class GetUserNotificationsQueryHandler 
    : IRequestHandler<GetUserNotificationsQuery, GetUserNotificationsResult>
{
    private readonly INotificationService _notificationService;

    public GetUserNotificationsQueryHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<GetUserNotificationsResult> Handle(
        GetUserNotificationsQuery query, 
        CancellationToken cancellationToken)
    {
        var notifications = await _notificationService.GetUserNotificationsAsync(
            query.UserId,
            query.UnreadOnly,
            query.PageNumber,
            query.PageSize
        );

        var notificationDtos = notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            Type = n.Type.ToString(),
            Title = n.Title,
            Message = n.Message,
            RelatedEntityType = n.RelatedEntityType,
            RelatedEntityId = n.RelatedEntityId,
            ActionUrl = n.ActionUrl,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt,
            ReadAt = n.ReadAt
        }).ToList();

        return new GetUserNotificationsResult
        {
            Notifications = notificationDtos
        };
    }
}

