using Microsoft.EntityFrameworkCore;
using PortalForge.Application.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;
using PortalForge.Infrastructure.Persistence;

namespace PortalForge.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for managing notification templates.
/// </summary>
public class NotificationTemplateRepository : INotificationTemplateRepository
{
    private readonly ApplicationDbContext _context;

    public NotificationTemplateRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationTemplate?> GetByIdAsync(Guid id)
    {
        return await _context.NotificationTemplates
            .Include(nt => nt.CreatedBy)
            .FirstOrDefaultAsync(nt => nt.Id == id);
    }

    public async Task<NotificationTemplate?> GetByTypeAndLanguageAsync(NotificationType type, string language = "pl")
    {
        return await _context.NotificationTemplates
            .Where(nt => nt.Type == type && nt.Language == language && nt.IsActive)
            .OrderByDescending(nt => nt.UpdatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<List<NotificationTemplate>> GetByTypeAsync(NotificationType type)
    {
        return await _context.NotificationTemplates
            .Include(nt => nt.CreatedBy)
            .Where(nt => nt.Type == type)
            .OrderByDescending(nt => nt.UpdatedAt)
            .ToListAsync();
    }

    public async Task<List<NotificationTemplate>> GetActiveTemplatesAsync()
    {
        return await _context.NotificationTemplates
            .Include(nt => nt.CreatedBy)
            .Where(nt => nt.IsActive)
            .OrderBy(nt => nt.Type)
            .ThenBy(nt => nt.Language)
            .ToListAsync();
    }

    public async Task CreateAsync(NotificationTemplate template)
    {
        await _context.NotificationTemplates.AddAsync(template);
    }

    public async Task UpdateAsync(NotificationTemplate template)
    {
        _context.NotificationTemplates.Update(template);
    }

    public async Task DeleteAsync(Guid id)
    {
        var template = await _context.NotificationTemplates.FindAsync(id);
        if (template != null)
        {
            _context.NotificationTemplates.Remove(template);
        }
    }

    public async Task<bool> ExistsAsync(NotificationType type, string language)
    {
        return await _context.NotificationTemplates
            .AnyAsync(nt => nt.Type == type && nt.Language == language && nt.IsActive);
    }
}