using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalForge.Application.UseCases.Notifications.Commands.MarkAllAsRead;
using PortalForge.Application.UseCases.Notifications.Commands.MarkAsRead;
using PortalForge.Application.UseCases.Notifications.Queries.GetUnreadCount;
using PortalForge.Application.UseCases.Notifications.Queries.GetUserNotifications;

namespace PortalForge.Api.Controllers;

[Route("api/notifications")]
[Authorize]
public class NotificationsController : BaseController
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get current user's notifications
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> GetNotifications(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var unauthorizedResult = GetUserIdOrUnauthorized(out var userGuid);
        if (unauthorizedResult != null)
        {
            return unauthorizedResult;
        }

        var query = new GetUserNotificationsQuery
        {
            UserId = userGuid,
            UnreadOnly = unreadOnly,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get count of unread notifications
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<ActionResult> GetUnreadCount()
    {
        var unauthorizedResult = GetUserIdOrUnauthorized(out var userGuid);
        if (unauthorizedResult != null)
        {
            return unauthorizedResult;
        }

        var query = new GetUnreadCountQuery
        {
            UserId = userGuid
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Mark a notification as read
    /// </summary>
    [HttpPatch("{id}/mark-read")]
    public async Task<ActionResult> MarkAsRead(Guid id)
    {
        var command = new MarkAsReadCommand
        {
            NotificationId = id
        };

        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Mark all notifications as read for current user
    /// </summary>
    [HttpPatch("mark-all-read")]
    public async Task<ActionResult> MarkAllAsRead()
    {
        var unauthorizedResult = GetUserIdOrUnauthorized(out var userGuid);
        if (unauthorizedResult != null)
        {
            return unauthorizedResult;
        }

        var command = new MarkAllAsReadCommand
        {
            UserId = userGuid
        };

        await _mediator.Send(command);
        return NoContent();
    }
}

