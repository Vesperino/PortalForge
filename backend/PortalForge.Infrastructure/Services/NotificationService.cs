using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public NotificationService(
        IUnitOfWork unitOfWork,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task CreateNotificationAsync(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        string? relatedEntityType = null,
        string? relatedEntityId = null,
        string? actionUrl = null)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            ActionUrl = actionUrl,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.NotificationRepository.CreateAsync(notification);
        await _unitOfWork.SaveChangesAsync();

        // Send email for important notifications
        await SendEmailForImportantNotificationsAsync(userId, type, title, message, actionUrl);
    }

    private async Task SendEmailForImportantNotificationsAsync(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        string? actionUrl)
    {
        // Only send emails for these important notification types
        var emailNotificationTypes = new[]
        {
            NotificationType.RequestPendingApproval,
            NotificationType.RequestApproved,
            NotificationType.RequestRejected,
            NotificationType.VacationCoverageAssigned,
            NotificationType.VacationCoverageStarted,
            NotificationType.VacationRequestCancelled,
            NotificationType.SickLeaveSubmitted,
            NotificationType.RequestRequiresCompletion,
            NotificationType.ApprovalOverdue,
            NotificationType.VacationExpiringSoon
        };

        if (!emailNotificationTypes.Contains(type))
        {
            return;
        }

        try
        {
            // Get user details
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.Email))
            {
                return;
            }

            var userName = $"{user.FirstName} {user.LastName}";
            var actionButtonText = GetActionButtonText(type);

            await _emailService.SendNotificationEmailAsync(
                toEmail: user.Email,
                toName: userName,
                notificationTitle: title,
                notificationMessage: message,
                actionUrl: actionUrl,
                actionButtonText: actionButtonText);
        }
        catch (Exception)
        {
            // Don't let email failures break notification creation
            // Logging is handled in EmailService
        }
    }

    private string? GetActionButtonText(NotificationType type)
    {
        return type switch
        {
            NotificationType.RequestPendingApproval => "Zobacz wniosek",
            NotificationType.RequestApproved => "Zobacz szczegóły",
            NotificationType.RequestRejected => "Zobacz szczegóły",
            NotificationType.VacationCoverageAssigned => "Zobacz zastępstwa",
            NotificationType.VacationCoverageStarted => "Zobacz zastępstwa",
            NotificationType.VacationRequestCancelled => "Zobacz szczegóły",
            NotificationType.SickLeaveSubmitted => "Zobacz L4",
            NotificationType.RequestRequiresCompletion => "Uzupełnij wniosek",
            NotificationType.ApprovalOverdue => "Zaakceptuj teraz",
            NotificationType.VacationExpiringSoon => "Zaplanuj urlop",
            _ => null
        };
    }

    public async Task NotifyApproverAsync(Guid approverId, Request request)
    {
        var submitter = request.SubmittedBy;
        var template = request.RequestTemplate;

        await CreateNotificationAsync(
            userId: approverId,
            type: NotificationType.RequestPendingApproval,
            title: $"Nowy wniosek do zatwierdzenia: {template.Name}",
            message: $"{submitter.FirstName} {submitter.LastName} przesłał wniosek \"{template.Name}\" oczekujący na Twoje zatwierdzenie.",
            relatedEntityType: "Request",
            relatedEntityId: request.Id.ToString(),
            actionUrl: $"/dashboard/requests/{request.Id}"
        );
    }

    public async Task NotifySubmitterAsync(Request request, string message, NotificationType type)
    {
        var template = request.RequestTemplate;

        await CreateNotificationAsync(
            userId: request.SubmittedById,
            type: type,
            title: $"Aktualizacja wniosku: {template.Name}",
            message: message,
            relatedEntityType: "Request",
            relatedEntityId: request.Id.ToString(),
            actionUrl: $"/dashboard/requests/{request.Id}"
        );
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(
        Guid userId,
        bool unreadOnly = false,
        int pageNumber = 1,
        int pageSize = 20)
    {
        return await _unitOfWork.NotificationRepository.GetUserNotificationsAsync(
            userId,
            unreadOnly,
            pageNumber,
            pageSize
        );
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _unitOfWork.NotificationRepository.GetUnreadCountAsync(userId);
    }

    public async Task MarkAsReadAsync(Guid notificationId)
    {
        await _unitOfWork.NotificationRepository.MarkAsReadAsync(notificationId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        await _unitOfWork.NotificationRepository.MarkAllAsReadAsync(userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task NotifySubstituteAsync(Guid substituteId, VacationSchedule schedule)
    {
        var user = schedule.User;

        await CreateNotificationAsync(
            userId: substituteId,
            type: NotificationType.VacationCoverageAssigned,
            title: "Zostałeś zastępcą podczas urlopu",
            message: $"Będziesz zastępcą dla {user.FirstName} {user.LastName} w okresie od {schedule.StartDate:yyyy-MM-dd} do {schedule.EndDate:yyyy-MM-dd}.",
            relatedEntityType: "VacationSchedule",
            relatedEntityId: schedule.Id.ToString(),
            actionUrl: $"/dashboard/vacations/substitutions"
        );
    }

    public async Task NotifyVacationStartedAsync(Guid substituteId, VacationSchedule schedule)
    {
        var user = schedule.User;

        await CreateNotificationAsync(
            userId: substituteId,
            type: NotificationType.VacationCoverageStarted,
            title: "Urlop rozpoczęty - aktywne zastępstwo",
            message: $"{user.FirstName} {user.LastName} rozpoczął urlop. Jesteś teraz jego zastępcą do {schedule.EndDate:yyyy-MM-dd}.",
            relatedEntityType: "VacationSchedule",
            relatedEntityId: schedule.Id.ToString(),
            actionUrl: $"/dashboard/vacations/substitutions"
        );
    }

    public async Task NotifyVacationEndedAsync(Guid userId, VacationSchedule schedule)
    {
        await CreateNotificationAsync(
            userId: userId,
            type: NotificationType.VacationEnded,
            title: "Urlop zakończony",
            message: $"Twój urlop zakończył się. Witamy z powrotem!",
            relatedEntityType: "VacationSchedule",
            relatedEntityId: schedule.Id.ToString(),
            actionUrl: $"/dashboard/vacations"
        );
    }

    public async Task SendVacationNotificationAsync(Guid userId, NotificationType type, Request request)
    {
        var template = request.RequestTemplate;
        var (title, message) = type switch
        {
            NotificationType.VacationRequestCancelled => (
                "Urlop anulowany",
                $"Twój wniosek urlopowy \"{template.Name}\" został anulowany."
            ),
            NotificationType.RequestApproved => (
                "Urlop zatwierdzony",
                $"Twój wniosek urlopowy \"{template.Name}\" został zatwierdzony."
            ),
            NotificationType.RequestRejected => (
                "Urlop odrzucony",
                $"Twój wniosek urlopowy \"{template.Name}\" został odrzucony."
            ),
            _ => ("Aktualizacja wniosku urlopowego", $"Status wniosku \"{template.Name}\" został zaktualizowany.")
        };

        await CreateNotificationAsync(
            userId: userId,
            type: type,
            title: title,
            message: message,
            relatedEntityType: "Request",
            relatedEntityId: request.Id.ToString(),
            actionUrl: $"/dashboard/requests/{request.Id}"
        );
    }

    public async Task SendSLAReminderAsync(Guid approverId, Request request, int daysOverdue)
    {
        var submitter = request.SubmittedBy;
        var template = request.RequestTemplate;

        await CreateNotificationAsync(
            userId: approverId,
            type: NotificationType.ApprovalOverdue,
            title: "⚠️ Wniosek wymaga pilnej akceptacji",
            message: $"Wniosek \"{template.Name}\" od {submitter.FirstName} {submitter.LastName} oczekuje już {daysOverdue} dni na Twoją akceptację. Proszę o szybkie rozpatrzenie.",
            relatedEntityType: "Request",
            relatedEntityId: request.Id.ToString(),
            actionUrl: $"/dashboard/requests/{request.Id}"
        );
    }

    public async Task SendRequestCompletionRequiredAsync(Guid userId, Request request, string reason)
    {
        var template = request.RequestTemplate;

        await CreateNotificationAsync(
            userId: userId,
            type: NotificationType.RequestRequiresCompletion,
            title: "Wniosek wymaga uzupełnienia",
            message: $"Twój wniosek \"{template.Name}\" wymaga uzupełnienia: {reason}",
            relatedEntityType: "Request",
            relatedEntityId: request.Id.ToString(),
            actionUrl: $"/dashboard/requests/{request.Id}/edit"
        );
    }

    public async Task SendVacationExpiryWarningAsync(Guid userId, DateTime expiryDate, int daysRemaining)
    {
        await CreateNotificationAsync(
            userId: userId,
            type: NotificationType.VacationExpiringSoon,
            title: "⚠️ Urlop zaległy wkrótce przepadnie",
            message: $"Masz {daysRemaining} dni urlopu zaległego, które przepadną {expiryDate:yyyy-MM-dd}. Zaplanuj urlop, aby nie stracić tych dni!",
            relatedEntityType: "User",
            relatedEntityId: userId.ToString(),
            actionUrl: "/dashboard/vacation/request"
        );
    }

    public async Task SendSickLeaveNotificationAsync(Guid supervisorId, SickLeave sickLeave)
    {
        var user = sickLeave.User;

        await CreateNotificationAsync(
            userId: supervisorId,
            type: NotificationType.SickLeaveSubmitted,
            title: "Zgłoszenie L4 - informacja",
            message: $"{user.FirstName} {user.LastName} zgłosił zwolnienie lekarskie (L4) od {sickLeave.StartDate:yyyy-MM-dd} do {sickLeave.EndDate:yyyy-MM-dd} ({sickLeave.DaysCount} dni). Wniosek został automatycznie zaakceptowany zgodnie z przepisami.",
            relatedEntityType: "SickLeave",
            relatedEntityId: sickLeave.Id.ToString(),
            actionUrl: $"/dashboard/sick-leaves/{sickLeave.Id}"
        );
    }
}


