using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.Common.Interfaces;

public interface IVacationScheduleRepository
{
    Task<VacationSchedule?> GetByIdAsync(Guid id);
    Task<IEnumerable<VacationSchedule>> GetAllAsync();
    Task<Guid> CreateAsync(VacationSchedule schedule);
    Task UpdateAsync(VacationSchedule schedule);
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Gets the active vacation for a user (if on vacation right now).
    /// </summary>
    Task<VacationSchedule?> GetActiveVacationAsync(Guid userId);

    /// <summary>
    /// Gets all vacations for a department in a date range.
    /// </summary>
    Task<List<VacationSchedule>> GetTeamVacationsAsync(
        Guid departmentId,
        DateTime startDate,
        DateTime endDate);

    /// <summary>
    /// Gets vacations where the user is the substitute.
    /// </summary>
    Task<List<VacationSchedule>> GetSubstitutionsAsync(Guid substituteId);

    /// <summary>
    /// Gets scheduled vacations that should be activated (StartDate &lt;= today).
    /// </summary>
    Task<List<VacationSchedule>> GetScheduledToActivateAsync();

    /// <summary>
    /// Gets active vacations that should be completed (EndDate &lt; today).
    /// </summary>
    Task<List<VacationSchedule>> GetActiveToCompleteAsync();
}
