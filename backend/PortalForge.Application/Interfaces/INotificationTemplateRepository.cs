using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.Interfaces;

/// <summary>
/// Repository for managing notification templates.
/// </summary>
public interface INotificationTemplateRepository
{
    /// <summary>
    /// Get notification template by ID.
    /// </summary>
    /// <param name="id">Template ID.</param>
    /// <returns>Template or null if not found.</returns>
    Task<NotificationTemplate?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Get active template for a specific notification type and language.
    /// </summary>
    /// <param name="type">Notification type.</param>
    /// <param name="language">Language code (default: "pl").</param>
    /// <returns>Template or null if not found.</returns>
    Task<NotificationTemplate?> GetByTypeAndLanguageAsync(NotificationType type, string language = "pl");
    
    /// <summary>
    /// Get all templates for a specific notification type.
    /// </summary>
    /// <param name="type">Notification type.</param>
    /// <returns>List of templates.</returns>
    Task<List<NotificationTemplate>> GetByTypeAsync(NotificationType type);
    
    /// <summary>
    /// Get all active templates.
    /// </summary>
    /// <returns>List of active templates.</returns>
    Task<List<NotificationTemplate>> GetActiveTemplatesAsync();
    
    /// <summary>
    /// Create new notification template.
    /// </summary>
    /// <param name="template">Template to create.</param>
    Task CreateAsync(NotificationTemplate template);
    
    /// <summary>
    /// Update existing notification template.
    /// </summary>
    /// <param name="template">Template to update.</param>
    Task UpdateAsync(NotificationTemplate template);
    
    /// <summary>
    /// Delete notification template.
    /// </summary>
    /// <param name="id">Template ID.</param>
    Task DeleteAsync(Guid id);
    
    /// <summary>
    /// Check if a template exists for a specific type and language.
    /// </summary>
    /// <param name="type">Notification type.</param>
    /// <param name="language">Language code.</param>
    /// <returns>True if template exists.</returns>
    Task<bool> ExistsAsync(NotificationType type, string language);
}