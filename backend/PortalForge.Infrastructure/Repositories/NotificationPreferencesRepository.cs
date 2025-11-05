using Microsoft.EntityFrameworkCore;
using PortalForge.Application.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;
using PortalForge.Infrastructure.Persistence;

namespace PortalForge.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for managing user notification preferences.
/// </summary>
public class NotificationPreferencesRepository : INotificationPreferencesRepository
{
    private readonly ApplicationDbContext _context;

    public NotificationPreferencesRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationPreferences?> GetByUserIdAsync(Guid userId)
    {
        return await _context.NotificationPreferences
            .FirstOrDefaultAsync(np => np.UserId == userId);
    }

    public async Task CreateAsync(NotificationPreferences preferences)
    {
        await _context.NotificationPreferences.AddAsync(preferences);
    }

    public async Task UpdateAsync(NotificationPreferences preferences)
    {
        _context.NotificationPreferences.Update(preferences);
    }

    public async Task DeleteAsync(Guid id)
    {
        var preferences = await _context.NotificationPreferences.FindAsync(id);
        if (preferences != null)
        {
            _context.NotificationPreferences.Remove(preferences);
        }
    }

    public async Task<List<Guid>> GetUsersWithDigestEnabledAsync(DigestFrequency frequency)
    {
        return await _context.NotificationPreferences
            .Where(np => np.DigestEnabled && np.DigestFrequency == frequency)
            .Select(np => np.UserId)
            .ToListAsync();
    }
}