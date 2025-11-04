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

    public async Task<VacationSchedule?> GetByIdAsync(Guid id)
    {
        return await _context.VacationSchedules
            .Include(v => v.User)
            .Include(v => v.Substitute)
            .Include(v => v.SourceRequest)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<IEnumerable<VacationSchedule>> GetAllAsync()
    {
        return await _context.VacationSchedules
            .Include(v => v.User)
            .Include(v => v.Substitute)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(VacationSchedule schedule)
    {
        _context.VacationSchedules.Add(schedule);
        return schedule.Id;
    }

    public async Task UpdateAsync(VacationSchedule schedule)
    {
        _context.VacationSchedules.Update(schedule);
    }

    public async Task DeleteAsync(Guid id)
    {
        var schedule = await _context.VacationSchedules.FindAsync(id);
        if (schedule != null)
        {
            _context.VacationSchedules.Remove(schedule);
        }
    }

    public async Task<VacationSchedule?> GetActiveVacationAsync(Guid userId)
    {
        var now = DateTime.UtcNow.Date;

        return await _context.VacationSchedules
            .Include(v => v.Substitute)
            .FirstOrDefaultAsync(v =>
                v.UserId == userId
                && v.StartDate <= now
                && v.EndDate >= now
                && v.Status == VacationStatus.Active
            );
    }

    public async Task<List<VacationSchedule>> GetTeamVacationsAsync(
        Guid departmentId,
        DateTime startDate,
        DateTime endDate)
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
            .ToListAsync();
    }

    public async Task<List<VacationSchedule>> GetSubstitutionsAsync(Guid substituteId)
    {
        return await _context.VacationSchedules
            .Include(v => v.User)
            .Where(v => v.SubstituteUserId == substituteId)
            .OrderBy(v => v.StartDate)
            .ToListAsync();
    }

    public async Task<List<VacationSchedule>> GetByUserAsync(Guid userId)
    {
        return await _context.VacationSchedules
            .Include(v => v.User)
            .Include(v => v.Substitute)
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.StartDate)
            .ToListAsync();
    }

    public async Task<List<VacationSchedule>> GetScheduledToActivateAsync()
    {
        var now = DateTime.UtcNow.Date;

        return await _context.VacationSchedules
            .Where(v => v.Status == VacationStatus.Scheduled && v.StartDate <= now)
            .ToListAsync();
    }

    public async Task<List<VacationSchedule>> GetActiveToCompleteAsync()
    {
        var now = DateTime.UtcNow.Date;

        return await _context.VacationSchedules
            .Where(v => v.Status == VacationStatus.Active && v.EndDate < now)
            .ToListAsync();
    }
}
