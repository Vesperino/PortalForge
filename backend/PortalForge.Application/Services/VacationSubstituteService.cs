using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.DTOs;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.Services;

/// <summary>
/// Service responsible for managing vacation substitutes and team calendar.
/// Handles finding active substitutes and generating team vacation views.
/// </summary>
public class VacationSubstituteService : IVacationSubstituteService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VacationSubstituteService> _logger;

    public VacationSubstituteService(
        IUnitOfWork unitOfWork,
        ILogger<VacationSubstituteService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<User?> GetActiveSubstituteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var vacation = await _unitOfWork.VacationScheduleRepository.GetActiveVacationAsync(userId);

        if (vacation != null)
        {
            _logger.LogDebug(
                "User {UserId} is on vacation, substitute is {SubstituteId}",
                userId, vacation.SubstituteUserId);

            return vacation.Substitute;
        }

        return null;
    }

    public async Task<VacationCalendar> GetTeamCalendarAsync(
        Guid departmentId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var vacations = await _unitOfWork.VacationScheduleRepository.GetTeamVacationsAsync(
            departmentId,
            startDate,
            endDate);

        var allUsers = await _unitOfWork.UserRepository.GetAllAsync();
        var teamSize = allUsers.Count(u => u.DepartmentId == departmentId && u.IsActive);

        var alerts = DetectConflicts(vacations, teamSize, startDate, endDate);
        var statistics = CalculateStatistics(vacations, teamSize);

        return new VacationCalendar
        {
            Vacations = vacations,
            TeamSize = teamSize,
            Alerts = alerts,
            Statistics = statistics
        };
    }

    public async Task<List<VacationSchedule>> GetMySubstitutionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.VacationScheduleRepository.GetSubstitutionsAsync(userId);
    }

    private static List<VacationAlert> DetectConflicts(
        List<VacationSchedule> vacations,
        int teamSize,
        DateTime rangeStart,
        DateTime rangeEnd)
    {
        var alerts = new List<VacationAlert>();

        if (teamSize == 0)
        {
            return alerts;
        }

        var currentDate = rangeStart.Date;
        while (currentDate <= rangeEnd.Date)
        {
            var onVacationCount = vacations.Count(v =>
                v.StartDate <= currentDate && v.EndDate >= currentDate);

            var coveragePercent = (double)onVacationCount / teamSize * 100;

            if (coveragePercent >= 30)
            {
                var affectedEmployees = vacations
                    .Where(v => v.StartDate <= currentDate && v.EndDate >= currentDate)
                    .Select(v => v.User)
                    .ToList();

                alerts.Add(new VacationAlert
                {
                    Date = currentDate,
                    Type = coveragePercent >= 50
                        ? AlertType.COVERAGE_CRITICAL
                        : AlertType.COVERAGE_LOW,
                    AffectedEmployees = affectedEmployees,
                    CoveragePercent = coveragePercent,
                    Message = $"{onVacationCount}/{teamSize} pracownikow na urlopie ({coveragePercent:F0}%)"
                });
            }

            currentDate = currentDate.AddDays(1);
        }

        return alerts;
    }

    private static VacationStatistics CalculateStatistics(
        List<VacationSchedule> vacations,
        int teamSize)
    {
        var currentlyOnVacation = vacations.Count(v => v.Status == VacationStatus.Active);
        var scheduledVacations = vacations.Count(v => v.Status == VacationStatus.Scheduled);
        var totalVacationDays = vacations.Sum(v => v.DaysCount);
        var averageVacationDays = vacations.Any()
            ? vacations.Average(v => v.DaysCount)
            : 0;

        return new VacationStatistics
        {
            CurrentlyOnVacation = currentlyOnVacation,
            ScheduledVacations = scheduledVacations,
            TotalVacationDays = totalVacationDays,
            AverageVacationDays = averageVacationDays,
            TeamSize = teamSize,
            CoveragePercent = teamSize > 0
                ? (double)(teamSize - currentlyOnVacation) / teamSize * 100
                : 100
        };
    }
}
