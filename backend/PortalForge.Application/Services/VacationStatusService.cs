using System.Text.Json;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.Services;

/// <summary>
/// Service responsible for managing vacation and sick leave status updates.
/// Handles activation and completion of vacations based on dates.
/// </summary>
public class VacationStatusService : IVacationStatusService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<VacationStatusService> _logger;

    public VacationStatusService(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<VacationStatusService> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task UpdateVacationStatusesAsync(CancellationToken cancellationToken = default)
    {
        var updated = 0;

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            updated += await ActivateScheduledVacationsAsync();
            updated += await CompleteActiveVacationsAsync();

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

    public async Task ProcessApprovedSickLeaveRequestsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing approved sick leave requests");

        var created = 0;

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var approvedRequests = await GetUnprocessedSickLeaveRequestsAsync();

            _logger.LogInformation(
                "Found {RequestCount} approved sick leave requests to process",
                approvedRequests.Count);

            foreach (var request in approvedRequests)
            {
                if (await TryProcessSickLeaveRequestAsync(request))
                {
                    created++;
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

    private async Task<int> ActivateScheduledVacationsAsync()
    {
        var toActivate = await _unitOfWork.VacationScheduleRepository.GetScheduledToActivateAsync();
        var count = 0;

        foreach (var vacation in toActivate)
        {
            vacation.Status = VacationStatus.Active;
            await _unitOfWork.VacationScheduleRepository.UpdateAsync(vacation);

            _logger.LogInformation(
                "Activated vacation for {UserId} - now on vacation until {EndDate}",
                vacation.UserId, vacation.EndDate);
            count++;

            if (vacation.SubstituteUserId.HasValue)
            {
                await _notificationService.NotifyVacationStartedAsync(
                    vacation.SubstituteUserId.Value,
                    vacation);
            }
        }

        return count;
    }

    private async Task<int> CompleteActiveVacationsAsync()
    {
        var toComplete = await _unitOfWork.VacationScheduleRepository.GetActiveToCompleteAsync();
        var count = 0;

        foreach (var vacation in toComplete)
        {
            vacation.Status = VacationStatus.Completed;
            await _unitOfWork.VacationScheduleRepository.UpdateAsync(vacation);

            _logger.LogInformation("Completed vacation for {UserId}", vacation.UserId);
            count++;

            await _notificationService.NotifyVacationEndedAsync(vacation.UserId, vacation);
        }

        return count;
    }

    private async Task<List<Request>> GetUnprocessedSickLeaveRequestsAsync()
    {
        var allRequests = await _unitOfWork.RequestRepository.GetAllAsync();
        var allSickLeaves = await _unitOfWork.SickLeaveRepository.GetAllAsync();

        return allRequests
            .Where(r => r.Status == RequestStatus.Approved &&
                       r.RequestTemplate.Name.Contains("L4") &&
                       !allSickLeaves.Any(sl => sl.SourceRequestId == r.Id))
            .ToList();
    }

    private async Task<bool> TryProcessSickLeaveRequestAsync(Request request)
    {
        try
        {
            var sickLeave = CreateSickLeaveFromRequest(request);
            await _unitOfWork.SickLeaveRepository.CreateAsync(sickLeave);

            _logger.LogInformation(
                "Created SickLeave record for user {UserId} from {StartDate} to {EndDate} ({DaysCount} days, ZUS required: {RequiresZus})",
                sickLeave.UserId, sickLeave.StartDate, sickLeave.EndDate, sickLeave.DaysCount, sickLeave.RequiresZusDocument);

            await SendSickLeaveNotificationsAsync(request, sickLeave);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process sick leave request {RequestId}", request.Id);
            return false;
        }
    }

    private static SickLeave CreateSickLeaveFromRequest(Request request)
    {
        var formData = JsonSerializer.Deserialize<Dictionary<string, object>>(request.FormData)
            ?? throw new InvalidOperationException("Invalid form data");

        var startDate = DateTime.Parse(formData["startDate"].ToString()!);
        var endDate = DateTime.Parse(formData["endDate"].ToString()!);
        var daysCount = (endDate.Date - startDate.Date).Days + 1;

        return new SickLeave
        {
            Id = Guid.NewGuid(),
            UserId = request.SubmittedById,
            StartDate = startDate.Date,
            EndDate = endDate.Date,
            DaysCount = daysCount,
            SourceRequestId = request.Id,
            Status = SickLeaveStatus.Active,
            RequiresZusDocument = daysCount > 33,
            CreatedAt = DateTime.UtcNow
        };
    }

    private async Task SendSickLeaveNotificationsAsync(Request request, SickLeave sickLeave)
    {
        if (sickLeave.RequiresZusDocument)
        {
            await _notificationService.CreateNotificationAsync(
                request.SubmittedById,
                NotificationType.System,
                "Wymagane zaswiadczenie ZUS",
                $"Twoje zwolnienie lekarskie przekracza 33 dni ({sickLeave.DaysCount} dni). Wymagane jest dostarczenie zaswiadczenia ZUS do dzialu HR.",
                "SickLeave",
                sickLeave.Id.ToString(),
                $"/dashboard/sick-leaves/{sickLeave.Id}");

            _logger.LogInformation(
                "Sent ZUS documentation reminder to user {UserId} for sick leave {SickLeaveId}",
                sickLeave.UserId, sickLeave.Id);
        }

        var user = await _unitOfWork.UserRepository.GetByIdAsync(request.SubmittedById);
        if (user?.DepartmentId.HasValue == true)
        {
            var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(user.DepartmentId.Value);
            if (department?.HeadOfDepartmentId.HasValue == true)
            {
                await _notificationService.SendSickLeaveNotificationAsync(
                    department.HeadOfDepartmentId.Value,
                    sickLeave);
            }
        }
    }
}
