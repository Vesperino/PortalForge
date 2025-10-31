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
            NotificationType.VacationCoverageStarted
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
}


