using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.Common.Interfaces;

public interface IVacationScheduleRepository
{
    Task<VacationSchedule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<VacationSchedule>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(VacationSchedule schedule, CancellationToken cancellationToken = default);
    Task UpdateAsync(VacationSchedule schedule, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the active vacation for a user (if on vacation right now).
    /// </summary>
    Task<VacationSchedule?> GetActiveVacationAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all vacations for a department in a date range.
    /// </summary>
    Task<List<VacationSchedule>> GetTeamVacationsAsync(
        Guid departmentId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets vacations where the user is the substitute.
    /// </summary>
    Task<List<VacationSchedule>> GetSubstitutionsAsync(Guid substituteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all vacations for a given user (any status), ordered by StartDate desc.
    /// </summary>
    Task<List<VacationSchedule>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets scheduled vacations that should be activated (StartDate &lt;= today).
    /// </summary>
    Task<List<VacationSchedule>> GetScheduledToActivateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active vacations that should be completed (EndDate &lt; today).
    /// </summary>
    Task<List<VacationSchedule>> GetActiveToCompleteAsync(CancellationToken cancellationToken = default);
}
