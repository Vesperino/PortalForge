using PortalForge.Domain.Entities;

namespace PortalForge.Application.Interfaces;

public interface ISystemSettingRepository
{
    Task<SystemSetting?> GetByIdAsync(int id);
    Task<SystemSetting?> GetByKeyAsync(string key);
    Task<IEnumerable<SystemSetting>> GetAllAsync();
    Task<IEnumerable<SystemSetting>> GetByCategoryAsync(string category);
    Task UpdateAsync(SystemSetting setting);
    Task UpdateBatchAsync(IEnumerable<SystemSetting> settings);
}
