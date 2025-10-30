using MediatR;
using PortalForge.Application.Services;

namespace PortalForge.Application.UseCases.Notifications.Queries.GetUnreadCount;

public class GetUnreadCountQueryHandler 
    : IRequestHandler<GetUnreadCountQuery, GetUnreadCountResult>
{
    private readonly INotificationService _notificationService;

    public GetUnreadCountQueryHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<GetUnreadCountResult> Handle(
        GetUnreadCountQuery query, 
        CancellationToken cancellationToken)
    {
        var count = await _notificationService.GetUnreadCountAsync(query.UserId);

        return new GetUnreadCountResult
        {
            Count = count
        };
    }
}

