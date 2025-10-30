using MediatR;

namespace PortalForge.Application.UseCases.Notifications.Queries.GetUnreadCount;

public class GetUnreadCountQuery : IRequest<GetUnreadCountResult>
{
    public Guid UserId { get; set; }
}

