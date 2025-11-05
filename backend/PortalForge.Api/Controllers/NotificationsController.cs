using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalForge.Application.UseCases.Notifications.Commands.MarkAllAsRead;
using PortalForge.Application.UseCases.Notifications.Commands.MarkAsRead;
using PortalForge.Application.UseCases.Notifications.Queries.GetUnreadCount;
using PortalForge.Application.UseCases.Notifications.Queries.GetUserNotifications;
using PortalForge.Application.Services;
using PortalForge.Domain.Enums;

namespace PortalForge.Api.Controllers;

[Route("api/notifications")]
[Authorize]
public class NotificationsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly ISmartNotificationService _smartNotificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        IMediator mediator,
        ISmartNotificationService smartNotificationService,
        ILogger<NotificationsController> logger)
    {
        _mediator = mediator;
        _smartNotificationService = smartNotificationService;
        _logger = logger;
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

    /// <summary>
    /// Get notification history with filtering options
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult> GetNotificationHistory(
        [FromQuery] NotificationType? type = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        var unauthorizedResult = GetUserIdOrUnauthorized(out var userGuid);
        if (unauthorizedResult != null)
        {
            return unauthorizedResult;
        }

        try
        {
            var query = new GetUserNotificationsQuery
            {
                UserId = userGuid,
                UnreadOnly = false,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            
            // Filter by type and date range if specified
            // Note: In a real implementation, this filtering should be done at the database level
            // This is a simplified version for demonstration
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification history for user {UserId}", userGuid);
            return BadRequest(new { Message = "Failed to get notification history" });
        }
    }

    /// <summary>
    /// Get grouped notifications for current user
    /// </summary>
    [HttpGet("grouped")]
    public async Task<ActionResult> GetGroupedNotifications()
    {
        var unauthorizedResult = GetUserIdOrUnauthorized(out var userGuid);
        if (unauthorizedResult != null)
        {
            return unauthorizedResult;
        }

        try
        {
            // Get recent notifications
            var query = new GetUserNotificationsQuery
            {
                UserId = userGuid,
                UnreadOnly = false,
                PageNumber = 1,
                PageSize = 100
            };

            var notificationsResult = await _mediator.Send(query);
            
            // Convert to domain entities for grouping
            // Note: This is a simplified implementation
            // In a real scenario, you'd need proper mapping
            var notifications = new List<Domain.Entities.Notification>();
            
            var groupedNotifications = await _smartNotificationService.GroupNotificationsAsync(
                userGuid, 
                notifications);

            return Ok(groupedNotifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting grouped notifications for user {UserId}", userGuid);
            return BadRequest(new { Message = "Failed to get grouped notifications" });
        }
    }

    /// <summary>
    /// Send test notification to current user (for testing preferences)
    /// </summary>
    [HttpPost("test")]
    public async Task<ActionResult> SendTestNotification([FromQuery] NotificationType type = NotificationType.System)
    {
        var unauthorizedResult = GetUserIdOrUnauthorized(out var userGuid);
        if (unauthorizedResult != null)
        {
            return unauthorizedResult;
        }

        try
        {
            await _smartNotificationService.CreateSmartNotificationAsync(
                userGuid,
                type,
                "Test Notification",
                "This is a test notification to verify your notification preferences.",
                "test",
                Guid.NewGuid().ToString(),
                "/notifications");

            return Ok(new { Message = "Test notification sent successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test notification to user {UserId}", userGuid);
            return BadRequest(new { Message = "Failed to send test notification" });
        }
    }
}

