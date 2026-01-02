using Microsoft.EntityFrameworkCore;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;
using PortalForge.Infrastructure.Persistence;

namespace PortalForge.Infrastructure.Repositories;

public class VacationScheduleRepository : IVacationScheduleRepository
{
    private readonly ApplicationDbContext _context;

    public VacationScheduleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<VacationSchedule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.VacationSchedules
            .Include(v => v.User)
            .Include(v => v.Substitute)
            .Include(v => v.SourceRequest)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<VacationSchedule>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.VacationSchedules
            .Include(v => v.User)
            .Include(v => v.Substitute)
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid> CreateAsync(VacationSchedule schedule, CancellationToken cancellationToken = default)
    {
        _context.VacationSchedules.Add(schedule);
        return schedule.Id;
    }

    public async Task UpdateAsync(VacationSchedule schedule, CancellationToken cancellationToken = default)
    {
        _context.VacationSchedules.Update(schedule);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var schedule = await _context.VacationSchedules.FindAsync(new object[] { id }, cancellationToken);
        if (schedule != null)
        {
            _context.VacationSchedules.Remove(schedule);
        }
    }

    public async Task<VacationSchedule?> GetActiveVacationAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow.Date;

        return await _context.VacationSchedules
            .Include(v => v.Substitute)
            .FirstOrDefaultAsync(v =>
                v.UserId == userId
                && v.StartDate <= now
                && v.EndDate >= now
                && v.Status == VacationStatus.Active
            , cancellationToken);
    }

    public async Task<List<VacationSchedule>> GetTeamVacationsAsync(
        Guid departmentId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.VacationSchedules
            .Include(v => v.User)
            .Include(v => v.Substitute)
            .Where(v =>
                v.User.DepartmentId == departmentId
                && v.EndDate >= startDate
                && v.StartDate <= endDate
                && v.Status != VacationStatus.Cancelled
            )
            .OrderBy(v => v.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<VacationSchedule>> GetSubstitutionsAsync(Guid substituteId, CancellationToken cancellationToken = default)
    {
        return await _context.VacationSchedules
            .Include(v => v.User)
            .Where(v => v.SubstituteUserId == substituteId)
            .OrderBy(v => v.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<VacationSchedule>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.VacationSchedules
            .Include(v => v.User)
            .Include(v => v.Substitute)
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<VacationSchedule>> GetScheduledToActivateAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow.Date;

        return await _context.VacationSchedules
            .Where(v => v.Status == VacationStatus.Scheduled && v.StartDate <= now)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<VacationSchedule>> GetActiveToCompleteAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow.Date;

        return await _context.VacationSchedules
            .Where(v => v.Status == VacationStatus.Active && v.EndDate < now)
            .ToListAsync(cancellationToken);
    }
}
