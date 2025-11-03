using System.Text.Json;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.DTOs;
using PortalForge.Application.Exceptions;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.Services;

/// <summary>
/// Service responsible for managing vacation schedules and substitute routing.
/// Handles automatic vacation creation, conflict detection, and status updates.
/// </summary>
public class VacationScheduleService : IVacationScheduleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<VacationScheduleService> _logger;

    public VacationScheduleService(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<VacationScheduleService> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task CreateFromApprovedRequestAsync(Request vacationRequest)
    {
        // 1. Parse form data
        var formData = JsonSerializer.Deserialize<Dictionary<string, object>>(
            vacationRequest.FormData
        ) ?? throw new InvalidOperationException("Invalid form data");

        // 2. Extract vacation details
        var startDate = DateTime.Parse(formData["startDate"].ToString()!);
        var endDate = DateTime.Parse(formData["endDate"].ToString()!);
        var substituteId = Guid.Parse(formData["substitute"].ToString()!);

        // 3. Validate substitute is not the user themselves
        if (substituteId == vacationRequest.SubmittedById)
        {
            _logger.LogWarning(
                "User {UserId} tried to set themselves as substitute",
                substituteId);
            throw new ValidationException("Nie możesz być własnym zastępcą");
        }

        // 4. Check if substitute is active
        var substitute = await _unitOfWork.UserRepository.GetByIdAsync(substituteId);
        if (substitute == null || !substitute.IsActive)
        {
            throw new NotFoundException(
                $"Zastępca {substituteId} nie istnieje lub jest nieaktywny");
        }

        // 5. Create vacation schedule
        var schedule = new VacationSchedule
        {
            Id = Guid.NewGuid(),
            UserId = vacationRequest.SubmittedById,
            StartDate = startDate.Date, // Ensure date only (no time)
            EndDate = endDate.Date,
            SubstituteUserId = substituteId,
            SourceRequestId = vacationRequest.Id,
            Status = VacationStatus.Scheduled,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.VacationScheduleRepository.CreateAsync(schedule);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Created vacation schedule for {UserId} from {StartDate} to {EndDate}, substitute: {SubstituteId}",
            schedule.UserId, schedule.StartDate, schedule.EndDate, schedule.SubstituteUserId
        );

        // 6. Send notification to substitute
        await _notificationService.NotifySubstituteAsync(substituteId, schedule);
    }

    public async Task<User?> GetActiveSubstituteAsync(Guid userId)
    {
        var vacation = await _unitOfWork.VacationScheduleRepository.GetActiveVacationAsync(userId);

        if (vacation != null)
        {
            _logger.LogDebug(
                "User {UserId} is on vacation, substitute is {SubstituteId}",
                userId, vacation.SubstituteUserId
            );

            return vacation.Substitute;
        }

        return null;
    }

    public async Task<VacationCalendar> GetTeamCalendarAsync(
        Guid departmentId,
        DateTime startDate,
        DateTime endDate)
    {
        // 1. Get all vacations in date range for department
        var vacations = await _unitOfWork.VacationScheduleRepository.GetTeamVacationsAsync(
            departmentId,
            startDate,
            endDate);

        // 2. Get team size - need to count active users in department
        var allUsers = await _unitOfWork.UserRepository.GetAllAsync();
        var teamSize = allUsers.Count(u => u.DepartmentId == departmentId && u.IsActive);

        // 3. Detect conflicts
        var alerts = DetectConflicts(vacations, teamSize, startDate, endDate);

        // 4. Calculate statistics
        var statistics = CalculateStatistics(vacations, teamSize);

        return new VacationCalendar
        {
            Vacations = vacations,
            TeamSize = teamSize,
            Alerts = alerts,
            Statistics = statistics
        };
    }

    public async Task<List<VacationSchedule>> GetMySubstitutionsAsync(Guid userId)
    {
        return await _unitOfWork.VacationScheduleRepository.GetSubstitutionsAsync(userId);
    }

    public async Task UpdateVacationStatusesAsync()
    {
        var updated = 0;

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // 1. Activate scheduled vacations (StartDate <= today)
            var toActivate = await _unitOfWork.VacationScheduleRepository.GetScheduledToActivateAsync();

            foreach (var vacation in toActivate)
            {
                vacation.Status = VacationStatus.Active;
                await _unitOfWork.VacationScheduleRepository.UpdateAsync(vacation);

                _logger.LogInformation(
                    "Activated vacation for {UserId} - now on vacation until {EndDate}",
                    vacation.UserId, vacation.EndDate
                );
                updated++;

                // Notify substitute
                await _notificationService.NotifyVacationStartedAsync(
                    vacation.SubstituteUserId,
                    vacation
                );
            }

            // 2. Complete active vacations (EndDate < today)
            var toComplete = await _unitOfWork.VacationScheduleRepository.GetActiveToCompleteAsync();

            foreach (var vacation in toComplete)
            {
                vacation.Status = VacationStatus.Completed;
                await _unitOfWork.VacationScheduleRepository.UpdateAsync(vacation);

                _logger.LogInformation(
                    "Completed vacation for {UserId}",
                    vacation.UserId
                );
                updated++;

                // Notify user vacation ended
                await _notificationService.NotifyVacationEndedAsync(
                    vacation.UserId,
                    vacation
                );
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Updated {Count} vacation statuses", updated);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error updating vacation statuses");
            throw;
        }
    }

    public async Task ProcessApprovedSickLeaveRequestsAsync()
    {
        _logger.LogInformation("Processing approved sick leave requests");

        var created = 0;

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Get all approved sick leave requests that don't have a SickLeave record yet
            var allRequests = await _unitOfWork.RequestRepository.GetAllAsync();
            var allSickLeaves = await _unitOfWork.SickLeaveRepository.GetAllAsync();

            var approvedSickLeaveRequests = allRequests
                .Where(r => r.Status == RequestStatus.Approved &&
                           r.RequestTemplate.Name.Contains("L4") && // Assuming sick leave templates contain "L4"
                           !allSickLeaves.Any(sl => sl.SourceRequestId == r.Id))
                .ToList();

            _logger.LogInformation(
                "Found {RequestCount} approved sick leave requests to process",
                approvedSickLeaveRequests.Count);

            foreach (var request in approvedSickLeaveRequests)
            {
                try
                {
                    // Parse form data
                    var formData = JsonSerializer.Deserialize<Dictionary<string, object>>(
                        request.FormData
                    ) ?? throw new InvalidOperationException("Invalid form data");

                    // Extract sick leave details
                    var startDate = DateTime.Parse(formData["startDate"].ToString()!);
                    var endDate = DateTime.Parse(formData["endDate"].ToString()!);

                    // Calculate business days (sick leave is always consecutive calendar days)
                    var daysCount = (endDate.Date - startDate.Date).Days + 1;

                    // Check if ZUS documentation is required (>33 days)
                    var requiresZusDocument = daysCount > 33;

                    // Create SickLeave record
                    var sickLeave = new SickLeave
                    {
                        Id = Guid.NewGuid(),
                        UserId = request.SubmittedById,
                        StartDate = startDate.Date,
                        EndDate = endDate.Date,
                        DaysCount = daysCount,
                        SourceRequestId = request.Id,
                        Status = SickLeaveStatus.Active,
                        RequiresZusDocument = requiresZusDocument,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.SickLeaveRepository.CreateAsync(sickLeave);

                    _logger.LogInformation(
                        "Created SickLeave record for user {UserId} from {StartDate} to {EndDate} ({DaysCount} days, ZUS required: {RequiresZus})",
                        sickLeave.UserId, sickLeave.StartDate, sickLeave.EndDate, sickLeave.DaysCount, requiresZusDocument);

                    created++;

                    // Send notification if ZUS documentation is required
                    if (requiresZusDocument)
                    {
                        await _notificationService.CreateNotificationAsync(
                            request.SubmittedById,
                            NotificationType.System,
                            "Wymagane zaświadczenie ZUS",
                            $"Twoje zwolnienie lekarskie przekracza 33 dni ({daysCount} dni). Wymagane jest dostarczenie zaświadczenia ZUS do działu HR.",
                            "SickLeave",
                            sickLeave.Id.ToString(),
                            $"/dashboard/sick-leaves/{sickLeave.Id}");

                        _logger.LogInformation(
                            "Sent ZUS documentation reminder to user {UserId} for sick leave {SickLeaveId}",
                            sickLeave.UserId, sickLeave.Id);
                    }

                    // Notify supervisor about sick leave (informational)
                    var user = await _unitOfWork.UserRepository.GetByIdAsync(request.SubmittedById);
                    if (user?.SupervisorId.HasValue == true)
                    {
                        await _notificationService.SendSickLeaveNotificationAsync(
                            user.SupervisorId.Value,
                            sickLeave);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to process sick leave request {RequestId}",
                        request.Id);
                    // Continue with other requests even if one fails
                }
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation(
                "Processed {CreatedCount} sick leave requests successfully",
                created);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error processing approved sick leave requests");
            throw;
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Detects vacation conflicts where >30% of team is on vacation.
    /// </summary>
    private List<VacationAlert> DetectConflicts(
        List<VacationSchedule> vacations,
        int teamSize,
        DateTime rangeStart,
        DateTime rangeEnd)
    {
        var alerts = new List<VacationAlert>();

        if (teamSize == 0) return alerts;

        // Iterate through each date in range
        var currentDate = rangeStart.Date;
        while (currentDate <= rangeEnd.Date)
        {
            // Count how many people are on vacation on this date
            var onVacationCount = vacations.Count(v =>
                v.StartDate <= currentDate && v.EndDate >= currentDate
            );

            var coveragePercent = (double)onVacationCount / teamSize * 100;

            // Alert if >30% of team on vacation
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
                    Message = $"⚠️ {onVacationCount}/{teamSize} pracowników na urlopie ({coveragePercent:F0}%)"
                });
            }

            currentDate = currentDate.AddDays(1);
        }

        return alerts;
    }

    /// <summary>
    /// Calculates statistical summary of vacation data.
    /// </summary>
    private VacationStatistics CalculateStatistics(
        List<VacationSchedule> vacations,
        int teamSize)
    {
        var now = DateTime.UtcNow.Date;

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

    #endregion
}
