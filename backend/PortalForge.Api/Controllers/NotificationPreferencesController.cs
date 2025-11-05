using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalForge.Api.DTOs.Requests.Notifications;
using PortalForge.Application.Services;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;
using System.Text.Json;

namespace PortalForge.Api.Controllers;

[Route("api/notification-preferences")]
[Authorize]
public class NotificationPreferencesController : BaseController
{
    private readonly ISmartNotificationService _smartNotificationService;
    private readonly ILogger<NotificationPreferencesController> _logger;

    public NotificationPreferencesController(
        ISmartNotificationService smartNotificationService,
        ILogger<NotificationPreferencesController> logger)
    {
        _smartNotificationService = smartNotificationService;
        _logger = logger;
    }

    /// <summary>
    /// Get current user's notification preferences
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> GetPreferences()
    {
        var unauthorizedResult = GetUserIdOrUnauthorized(out var userGuid);
        if (unauthorizedResult != null)
        {
            return unauthorizedResult;
        }

        try
        {
            var preferences = await _smartNotificationService.GetUserPreferencesAsync(userGuid);
            return Ok(preferences);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification preferences for user {UserId}", userGuid);
            return BadRequest(new { Message = "Failed to get notification preferences" });
        }
    }

    /// <summary>
    /// Update current user's notification preferences
    /// </summary>
    [HttpPut]
    public async Task<ActionResult> UpdatePreferences([FromBody] UpdateNotificationPreferencesDto dto)
    {
        var unauthorizedResult = GetUserIdOrUnauthorized(out var userGuid);
        if (unauthorizedResult != null)
        {
            return unauthorizedResult;
        }

        try
        {
            var preferences = new NotificationPreferences
            {
                UserId = userGuid,
                EmailEnabled = dto.EmailEnabled,
                InAppEnabled = dto.InAppEnabled,
                DigestEnabled = dto.DigestEnabled,
                DigestFrequency = dto.DigestFrequency,
                DisabledTypes = JsonSerializer.Serialize(dto.DisabledTypes),
                GroupSimilarNotifications = dto.GroupSimilarNotifications,
                MaxGroupSize = dto.MaxGroupSize,
                GroupingTimeWindowMinutes = dto.GroupingTimeWindowMinutes,
                RealTimeEnabled = dto.RealTimeEnabled,
                UpdatedAt = DateTime.UtcNow
            };

            await _smartNotificationService.UpdateUserPreferencesAsync(userGuid, preferences);
            return Ok(new { Message = "Notification preferences updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating notification preferences for user {UserId}", userGuid);
            return BadRequest(new { Message = "Failed to update notification preferences" });
        }
    }

    /// <summary>
    /// Get digest configuration for current user
    /// </summary>
    [HttpGet("digest-config")]
    public async Task<ActionResult> GetDigestConfig()
    {
        var unauthorizedResult = GetUserIdOrUnauthorized(out var userGuid);
        if (unauthorizedResult != null)
        {
            return unauthorizedResult;
        }

        try
        {
            var preferences = await _smartNotificationService.GetUserPreferencesAsync(userGuid);
            var config = new
            {
                DigestEnabled = preferences.DigestEnabled,
                DigestFrequency = preferences.DigestFrequency,
                GroupSimilarNotifications = preferences.GroupSimilarNotifications,
                MaxGroupSize = preferences.MaxGroupSize,
                GroupingTimeWindowMinutes = preferences.GroupingTimeWindowMinutes
            };

            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting digest configuration for user {UserId}", userGuid);
            return BadRequest(new { Message = "Failed to get digest configuration" });
        }
    }

    /// <summary>
    /// Generate digest preview for current user
    /// </summary>
    [HttpGet("digest-preview")]
    public async Task<ActionResult> GetDigestPreview(
        [FromQuery] DigestType type = DigestType.Daily)
    {
        var unauthorizedResult = GetUserIdOrUnauthorized(out var userGuid);
        if (unauthorizedResult != null)
        {
            return unauthorizedResult;
        }

        try
        {
            var periodStart = type == DigestType.Daily 
                ? DateTime.UtcNow.Date.AddDays(-1)
                : DateTime.UtcNow.Date.AddDays(-7);
            
            var periodEnd = DateTime.UtcNow;

            var digest = await _smartNotificationService.GenerateDigestAsync(
                userGuid, 
                periodStart, 
                periodEnd);

            return Ok(digest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating digest preview for user {UserId}", userGuid);
            return BadRequest(new { Message = "Failed to generate digest preview" });
        }
    }

    /// <summary>
    /// Check if a specific notification type is enabled for current user
    /// </summary>
    [HttpGet("type-enabled/{type}")]
    public async Task<ActionResult> IsNotificationTypeEnabled(NotificationType type)
    {
        var unauthorizedResult = GetUserIdOrUnauthorized(out var userGuid);
        if (unauthorizedResult != null)
        {
            return unauthorizedResult;
        }

        try
        {
            var isEnabled = await _smartNotificationService.IsNotificationTypeEnabledAsync(userGuid, type);
            return Ok(new { Type = type, Enabled = isEnabled });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking notification type {Type} for user {UserId}", type, userGuid);
            return BadRequest(new { Message = "Failed to check notification type status" });
        }
    }
}