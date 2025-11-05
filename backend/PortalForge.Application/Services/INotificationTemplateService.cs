using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.Services;

/// <summary>
/// Service for managing notification templates and rendering notifications.
/// </summary>
public interface INotificationTemplateService
{
    /// <summary>
    /// Render a notification using a template.
    /// </summary>
    /// <param name="type">Notification type.</param>
    /// <param name="placeholders">Dictionary of placeholder values.</param>
    /// <param name="language">Language code (default: "pl").</param>
    /// <returns>Rendered notification with title and message.</returns>
    Task<(string title, string message)> RenderNotificationAsync(
        NotificationType type, 
        Dictionary<string, object> placeholders, 
        string language = "pl");
    
    /// <summary>
    /// Render an email notification using a template.
    /// </summary>
    /// <param name="type">Notification type.</param>
    /// <param name="placeholders">Dictionary of placeholder values.</param>
    /// <param name="language">Language code (default: "pl").</param>
    /// <returns>Rendered email with subject and body.</returns>
    Task<(string subject, string body)> RenderEmailNotificationAsync(
        NotificationType type, 
        Dictionary<string, object> placeholders, 
        string language = "pl");
    
    /// <summary>
    /// Get or create default template for a notification type.
    /// </summary>
    /// <param name="type">Notification type.</param>
    /// <param name="language">Language code (default: "pl").</param>
    /// <returns>Template for the notification type.</returns>
    Task<NotificationTemplate> GetOrCreateDefaultTemplateAsync(NotificationType type, string language = "pl");
    
    /// <summary>
    /// Create default templates for all notification types.
    /// </summary>
    /// <param name="language">Language code (default: "pl").</param>
    Task CreateDefaultTemplatesAsync(string language = "pl");
    
    /// <summary>
    /// Validate template placeholders.
    /// </summary>
    /// <param name="template">Template to validate.</param>
    /// <param name="placeholders">Available placeholders.</param>
    /// <returns>List of validation errors.</returns>
    Task<List<string>> ValidateTemplateAsync(NotificationTemplate template, Dictionary<string, object> placeholders);
}