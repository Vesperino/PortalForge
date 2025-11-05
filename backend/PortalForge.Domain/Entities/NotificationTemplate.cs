using PortalForge.Domain.Enums;

namespace PortalForge.Domain.Entities;

/// <summary>
/// Template for generating notifications with consistent formatting.
/// </summary>
public class NotificationTemplate
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Type of notification this template is for.
    /// </summary>
    public NotificationType Type { get; set; }
    
    /// <summary>
    /// Template name for identification.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Template for the notification title with placeholders.
    /// </summary>
    public string TitleTemplate { get; set; } = string.Empty;
    
    /// <summary>
    /// Template for the notification message with placeholders.
    /// </summary>
    public string MessageTemplate { get; set; } = string.Empty;
    
    /// <summary>
    /// Template for the email subject when sending email notifications.
    /// </summary>
    public string? EmailSubjectTemplate { get; set; }
    
    /// <summary>
    /// Template for the email body when sending email notifications.
    /// </summary>
    public string? EmailBodyTemplate { get; set; }
    
    /// <summary>
    /// Whether this template is active and should be used.
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Language code for this template (e.g., "pl", "en").
    /// </summary>
    public string Language { get; set; } = "pl";
    
    /// <summary>
    /// JSON object defining available placeholders and their descriptions.
    /// </summary>
    public string? PlaceholderDefinitions { get; set; }
    
    /// <summary>
    /// When this template was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// When this template was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// User who created this template.
    /// </summary>
    public Guid? CreatedById { get; set; }
    public User? CreatedBy { get; set; }
}