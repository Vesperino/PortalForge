using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.Interfaces;

/// <summary>
/// Repository for managing user notification preferences.
/// </summary>
public interface INotificationPreferencesRepository
{
    /// <summary>
    /// Get notification preferences for a user.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <returns>User's preferences or null if none exist.</returns>
    Task<NotificationPreferences?> GetByUserIdAsync(Guid userId);
    
    /// <summary>
    /// Create new notification preferences.
    /// </summary>
    /// <param name="preferences">Preferences to create.</param>
    Task CreateAsync(NotificationPreferences preferences);
    
    /// <summary>
    /// Update existing notification preferences.
    /// </summary>
    /// <param name="preferences">Preferences to update.</param>
    Task UpdateAsync(NotificationPreferences preferences);
    
    /// <summary>
    /// Delete notification preferences.
    /// </summary>
    /// <param name="id">Preferences ID.</param>
    Task DeleteAsync(Guid id);
    
    /// <summary>
    /// Get all users who have digest notifications enabled for a specific frequency.
    /// </summary>
    /// <param name="frequency">Digest frequency.</param>
    /// <returns>List of user IDs with digest enabled.</returns>
    Task<List<Guid>> GetUsersWithDigestEnabledAsync(DigestFrequency frequency);
}