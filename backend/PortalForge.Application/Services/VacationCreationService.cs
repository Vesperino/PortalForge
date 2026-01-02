using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.Services;

/// <summary>
/// Service responsible for creating vacation schedules from approved requests.
/// Handles extraction of vacation details from form data and validation.
/// </summary>
public class VacationCreationService : IVacationCreationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<VacationCreationService> _logger;

    public VacationCreationService(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<VacationCreationService> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task CreateFromApprovedRequestAsync(
        Request vacationRequest,
        CancellationToken cancellationToken = default)
    {
        var formData = ParseFormData(vacationRequest.FormData);
        var (startDate, endDate, substituteId) = ExtractVacationDetails(formData, vacationRequest.SubmittedById);

        ValidateSubstituteIsNotSelf(substituteId, vacationRequest.SubmittedById);
        substituteId = await ValidateAndGetSubstituteAsync(substituteId);

        var schedule = CreateVacationSchedule(vacationRequest, startDate, endDate, substituteId);

        await _unitOfWork.VacationScheduleRepository.CreateAsync(schedule);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Created vacation schedule for {UserId} from {StartDate} to {EndDate}, substitute: {SubstituteId}",
            schedule.UserId, schedule.StartDate, schedule.EndDate, schedule.SubstituteUserId);

        if (substituteId.HasValue)
        {
            await _notificationService.NotifySubstituteAsync(substituteId.Value, schedule);
        }
    }

    private static Dictionary<string, object> ParseFormData(string formDataJson)
    {
        return JsonSerializer.Deserialize<Dictionary<string, object>>(formDataJson)
            ?? throw new InvalidOperationException("Invalid form data");
    }

    private static (DateTime StartDate, DateTime EndDate, Guid? SubstituteId) ExtractVacationDetails(
        Dictionary<string, object> formData,
        Guid submitterId)
    {
        DateTime? startDate = null;
        DateTime? endDate = null;
        Guid? substituteId = null;

        if (formData.TryGetValue("substituteUserId", out var subRaw) && subRaw != null)
        {
            var str = subRaw.ToString();
            if (!string.IsNullOrWhiteSpace(str) && Guid.TryParse(str, out var sid) && sid != submitterId)
            {
                substituteId = sid;
            }
        }

        foreach (var kv in formData)
        {
            var v = kv.Value?.ToString();
            if (string.IsNullOrWhiteSpace(v))
            {
                continue;
            }

            if (Regex.IsMatch(v, @"^\d{4}-\d{2}-\d{2}$"))
            {
                if (!startDate.HasValue)
                {
                    startDate = DateTime.Parse(v);
                }
                else if (!endDate.HasValue)
                {
                    endDate = DateTime.Parse(v);
                }
                continue;
            }

            if (!substituteId.HasValue && Guid.TryParse(v, out var gid) && gid != submitterId)
            {
                substituteId = gid;
            }
        }

        if (!startDate.HasValue || !endDate.HasValue)
        {
            throw new ValidationException("Brak dat urlopu w danych wniosku");
        }

        return (startDate.Value, endDate.Value, substituteId);
    }

    private void ValidateSubstituteIsNotSelf(Guid? substituteId, Guid submitterId)
    {
        if (substituteId.HasValue && substituteId.Value == submitterId)
        {
            _logger.LogWarning(
                "User {UserId} tried to set themselves as substitute",
                substituteId);
            throw new ValidationException("Nie mozesz byc wlasnym zastepca");
        }
    }

    private async Task<Guid?> ValidateAndGetSubstituteAsync(Guid? substituteId)
    {
        if (!substituteId.HasValue)
        {
            return null;
        }

        var substitute = await _unitOfWork.UserRepository.GetByIdAsync(substituteId.Value);
        if (substitute == null || !substitute.IsActive)
        {
            return null;
        }

        return substituteId;
    }

    private static VacationSchedule CreateVacationSchedule(
        Request vacationRequest,
        DateTime startDate,
        DateTime endDate,
        Guid? substituteId)
    {
        return new VacationSchedule
        {
            Id = Guid.NewGuid(),
            UserId = vacationRequest.SubmittedById,
            StartDate = startDate.Date,
            EndDate = endDate.Date,
            SubstituteUserId = substituteId,
            SourceRequestId = vacationRequest.Id,
            Status = VacationStatus.Scheduled,
            CreatedAt = DateTime.UtcNow
        };
    }
}
