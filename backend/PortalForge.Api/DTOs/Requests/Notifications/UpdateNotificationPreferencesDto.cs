using PortalForge.Domain.Enums;

namespace PortalForge.Api.DTOs.Requests.Notifications;

/// <summary>
/// DTO for updating notification preferences
/// </summary>
public class UpdateNotificationPreferencesDto
{
    public bool EmailEnabled { get; set; } = true;
    public bool InAppEnabled { get; set; } = true;
    public bool DigestEnabled { get; set; } = false;
    public DigestFrequency DigestFrequency { get; set; } = DigestFrequency.Daily;
    public List<NotificationType> DisabledTypes { get; set; } = new();
    public bool GroupSimilarNotifications { get; set; } = true;
    public int MaxGroupSize { get; set; } = 5;
    public int GroupingTimeWindowMinutes { get; set; } = 60;
    public bool RealTimeEnabled { get; set; } = true;
}