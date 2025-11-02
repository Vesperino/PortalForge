using Microsoft.EntityFrameworkCore;
using PortalForge.Application.Interfaces;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Persistence.Repositories;

public class SystemSettingRepository : ISystemSettingRepository
{
    private readonly ApplicationDbContext _context;

    public SystemSettingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SystemSetting?> GetByIdAsync(int id)
    {
        return await _context.SystemSettings.FindAsync(id);
    }

    public async Task<SystemSetting?> GetByKeyAsync(string key)
    {
        return await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.Key == key);
    }

    public async Task<IEnumerable<SystemSetting>> GetAllAsync()
    {
        return await _context.SystemSettings
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Key)
            .ToListAsync();
    }

    public async Task<IEnumerable<SystemSetting>> GetByCategoryAsync(string category)
    {
        return await _context.SystemSettings
            .Where(s => s.Category == category)
            .OrderBy(s => s.Key)
            .ToListAsync();
    }

    public async Task UpdateAsync(SystemSetting setting)
    {
        _context.SystemSettings.Update(setting);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateBatchAsync(IEnumerable<SystemSetting> settings)
    {
        _context.SystemSettings.UpdateRange(settings);
        await _context.SaveChangesAsync();
    }
}
