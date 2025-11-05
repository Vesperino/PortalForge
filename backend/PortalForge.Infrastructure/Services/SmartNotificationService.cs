using System.Text.Json;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.DTOs;
using PortalForge.Application.Services;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Infrastructure.Services;

/// <summary>
/// Enhanced notification service with smart grouping, preferences, and real-time capabilities.
/// </summary>
public class SmartNotificationService : NotificationService, ISmartNotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly INotificationTemplateService _templateService;

    public SmartNotificationService(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        INotificationTemplateService templateService) : base(unitOfWork, emailService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _templateService = templateService;
    }

    public async Task SendGroupedNotificationsAsync(Guid userId, List<NotificationGroupDto> groups)
    {
        var preferences = await GetUserPreferencesAsync(userId);
        
        if (!preferences.InAppEnabled)
        {
            return;
        }

        foreach (var group in groups)
        {
            // Check if this notification type is enabled
            if (!await IsNotificationTypeEnabledAsync(userId, group.Type))
            {
                continue;
            }

            // Create a single notification for the group
            await CreateNotificationAsync(
                userId: userId,
                type: group.Type,
                title: group.Title,
                message: group.Message,
                relatedEntityType: "NotificationGroup",
                relatedEntityId: string.Join(",", group.NotificationIds),
                actionUrl: group.ActionUrl
            );

            // Send real-time notification if enabled
            if (preferences.RealTimeEnabled)
            {
                await SendRealTimeNotificationAsync(userId, group.Message, group.Type);
            }
        }
    }

    public async Task<NotificationPreferences> GetUserPreferencesAsync(Guid userId)
    {
        var preferences = await _unitOfWork.NotificationPreferencesRepository.GetByUserIdAsync(userId);
        
        if (preferences == null)
        {
            // Create default preferences
            preferences = new NotificationPreferences
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                EmailEnabled = true,
                InAppEnabled = true,
                DigestEnabled = false,
                DigestFrequency = DigestFrequency.Daily,
                DisabledTypes = "[]",
                GroupSimilarNotifications = true,
                MaxGroupSize = 5,
                GroupingTimeWindowMinutes = 60,
                RealTimeEnabled = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            await _unitOfWork.NotificationPreferencesRepository.CreateAsync(preferences);
            await _unitOfWork.SaveChangesAsync();
        }
        
        return preferences;
    }

    public async Task UpdateUserPreferencesAsync(Guid userId, NotificationPreferences preferences)
    {
        var existingPreferences = await _unitOfWork.NotificationPreferencesRepository.GetByUserIdAsync(userId);
        
        if (existingPreferences == null)
        {
            preferences.Id = Guid.NewGuid();
            preferences.UserId = userId;
            preferences.CreatedAt = DateTime.UtcNow;
            preferences.UpdatedAt = DateTime.UtcNow;
            
            await _unitOfWork.NotificationPreferencesRepository.CreateAsync(preferences);
        }
        else
        {
            existingPreferences.EmailEnabled = preferences.EmailEnabled;
            existingPreferences.InAppEnabled = preferences.InAppEnabled;
            existingPreferences.DigestEnabled = preferences.DigestEnabled;
            existingPreferences.DigestFrequency = preferences.DigestFrequency;
            existingPreferences.DisabledTypes = preferences.DisabledTypes;
            existingPreferences.GroupSimilarNotifications = preferences.GroupSimilarNotifications;
            existingPreferences.MaxGroupSize = preferences.MaxGroupSize;
            existingPreferences.GroupingTimeWindowMinutes = preferences.GroupingTimeWindowMinutes;
            existingPreferences.RealTimeEnabled = preferences.RealTimeEnabled;
            existingPreferences.UpdatedAt = DateTime.UtcNow;
            
            await _unitOfWork.NotificationPreferencesRepository.UpdateAsync(existingPreferences);
        }
        
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SendDigestNotificationAsync(Guid userId, DigestType type)
    {
        var preferences = await GetUserPreferencesAsync(userId);
        
        if (!preferences.DigestEnabled)
        {
            return;
        }

        var (periodStart, periodEnd) = GetDigestPeriod(type);
        var digest = await GenerateDigestAsync(userId, periodStart, periodEnd);
        
        if (digest.TotalNotifications == 0)
        {
            return;
        }

        var title = type == DigestType.Daily ? "Podsumowanie dnia" : "Podsumowanie tygodnia";
        
        await CreateNotificationAsync(
            userId: userId,
            type: NotificationType.System,
            title: title,
            message: digest.Summary,
            relatedEntityType: "Digest",
            relatedEntityId: type.ToString(),
            actionUrl: "/dashboard/notifications"
        );

        // Send email digest if enabled
        if (preferences.EmailEnabled)
        {
            await SendDigestEmailAsync(userId, digest);
        }
    }

    public async Task SendRealTimeNotificationAsync(Guid userId, string message, NotificationType type)
    {
        var preferences = await GetUserPreferencesAsync(userId);
        
        if (!preferences.RealTimeEnabled || !await IsNotificationTypeEnabledAsync(userId, type))
        {
            return;
        }

        // TODO: Implement real-time notification using SignalR or similar technology
        // For now, this is a placeholder that could be extended with SignalR integration
        
        // Log the real-time notification attempt
        Console.WriteLine($"Real-time notification for user {userId}: {message}");
    }

    public async Task<List<NotificationGroupDto>> GroupNotificationsAsync(Guid userId, List<Notification> notifications)
    {
        var preferences = await GetUserPreferencesAsync(userId);
        
        if (!preferences.GroupSimilarNotifications)
        {
            // Return individual notifications as single-item groups
            return notifications.Select(n => new NotificationGroupDto
            {
                Type = n.Type,
                Count = 1,
                Title = n.Title,
                Message = n.Message,
                ActionUrl = n.ActionUrl,
                FirstCreatedAt = n.CreatedAt,
                LastCreatedAt = n.CreatedAt,
                NotificationIds = new List<Guid> { n.Id }
            }).ToList();
        }

        var groups = new List<NotificationGroupDto>();
        var timeWindow = TimeSpan.FromMinutes(preferences.GroupingTimeWindowMinutes);
        
        // Group notifications by type and time window
        var notificationsByType = notifications
            .GroupBy(n => n.Type)
            .ToList();

        foreach (var typeGroup in notificationsByType)
        {
            var sortedNotifications = typeGroup.OrderBy(n => n.CreatedAt).ToList();
            var currentGroup = new List<Notification>();
            
            foreach (var notification in sortedNotifications)
            {
                if (currentGroup.Count == 0)
                {
                    currentGroup.Add(notification);
                }
                else
                {
                    var timeDiff = notification.CreatedAt - currentGroup.Last().CreatedAt;
                    
                    if (timeDiff <= timeWindow && currentGroup.Count < preferences.MaxGroupSize)
                    {
                        currentGroup.Add(notification);
                    }
                    else
                    {
                        // Create group from current notifications
                        groups.Add(CreateNotificationGroup(currentGroup));
                        
                        // Start new group
                        currentGroup = new List<Notification> { notification };
                    }
                }
            }
            
            // Add the last group
            if (currentGroup.Count > 0)
            {
                groups.Add(CreateNotificationGroup(currentGroup));
            }
        }

        return groups;
    }

    public async Task<bool> IsNotificationTypeEnabledAsync(Guid userId, NotificationType type)
    {
        var preferences = await GetUserPreferencesAsync(userId);
        
        if (string.IsNullOrEmpty(preferences.DisabledTypes))
        {
            return true;
        }

        try
        {
            var disabledTypes = JsonSerializer.Deserialize<List<NotificationType>>(preferences.DisabledTypes);
            return disabledTypes == null || !disabledTypes.Contains(type);
        }
        catch
        {
            // If JSON parsing fails, assume all types are enabled
            return true;
        }
    }

    public async Task<DigestNotificationDto> GenerateDigestAsync(Guid userId, DateTime periodStart, DateTime periodEnd)
    {
        var notifications = await _unitOfWork.NotificationRepository.GetUserNotificationsAsync(
            userId, 
            unreadOnly: false, 
            pageNumber: 1, 
            pageSize: 1000
        );

        var periodNotifications = notifications
            .Where(n => n.CreatedAt >= periodStart && n.CreatedAt <= periodEnd)
            .ToList();

        var groups = await GroupNotificationsAsync(userId, periodNotifications);
        
        var digest = new DigestNotificationDto
        {
            Type = periodEnd.Subtract(periodStart).Days > 1 ? DigestType.Weekly : DigestType.Daily,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            TotalNotifications = periodNotifications.Count,
            Groups = groups,
            Summary = GenerateDigestSummary(groups, periodNotifications.Count)
        };

        return digest;
    }

    public async Task CreateSmartNotificationAsync(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        string? relatedEntityType = null,
        string? relatedEntityId = null,
        string? actionUrl = null)
    {
        // Check if notification type is enabled for user
        if (!await IsNotificationTypeEnabledAsync(userId, type))
        {
            return;
        }

        var preferences = await GetUserPreferencesAsync(userId);
        
        // Create the notification
        await CreateNotificationAsync(userId, type, title, message, relatedEntityType, relatedEntityId, actionUrl);
        
        // Send real-time notification if enabled
        if (preferences.RealTimeEnabled)
        {
            await SendRealTimeNotificationAsync(userId, message, type);
        }
    }

    /// <summary>
    /// Create notification using template with placeholders.
    /// </summary>
    public async Task CreateTemplatedNotificationAsync(
        Guid userId,
        NotificationType type,
        Dictionary<string, object> placeholders,
        string? relatedEntityType = null,
        string? relatedEntityId = null,
        string? actionUrl = null)
    {
        // Check if notification type is enabled for user
        if (!await IsNotificationTypeEnabledAsync(userId, type))
        {
            return;
        }

        var preferences = await GetUserPreferencesAsync(userId);
        
        // Render notification using template
        var (title, message) = await _templateService.RenderNotificationAsync(type, placeholders, "pl");
        
        // Create the notification
        await CreateNotificationAsync(userId, type, title, message, relatedEntityType, relatedEntityId, actionUrl);
        
        // Send real-time notification if enabled
        if (preferences.RealTimeEnabled)
        {
            await SendRealTimeNotificationAsync(userId, message, type);
        }
    }

    private NotificationGroupDto CreateNotificationGroup(List<Notification> notifications)
    {
        var first = notifications.First();
        var last = notifications.Last();
        
        var title = notifications.Count == 1 
            ? first.Title 
            : $"{first.Title} (+{notifications.Count - 1} więcej)";
            
        var message = notifications.Count == 1
            ? first.Message
            : $"Masz {notifications.Count} powiadomień typu {GetNotificationTypeDisplayName(first.Type)}";

        return new NotificationGroupDto
        {
            Type = first.Type,
            Count = notifications.Count,
            Title = title,
            Message = message,
            ActionUrl = notifications.Count == 1 ? first.ActionUrl : "/dashboard/notifications",
            FirstCreatedAt = first.CreatedAt,
            LastCreatedAt = last.CreatedAt,
            NotificationIds = notifications.Select(n => n.Id).ToList()
        };
    }

    private string GetNotificationTypeDisplayName(NotificationType type)
    {
        return type switch
        {
            NotificationType.RequestPendingApproval => "oczekujące zatwierdzenie",
            NotificationType.RequestApproved => "zatwierdzone wnioski",
            NotificationType.RequestRejected => "odrzucone wnioski",
            NotificationType.RequestCompleted => "zakończone wnioski",
            NotificationType.RequestCommented => "komentarze do wniosków",
            NotificationType.VacationCoverageAssigned => "przypisane zastępstwa",
            NotificationType.VacationCoverageStarted => "rozpoczęte zastępstwa",
            NotificationType.VacationEnded => "zakończone urlopy",
            NotificationType.VacationRequestCancelled => "anulowane urlopy",
            NotificationType.SickLeaveSubmitted => "zgłoszone L4",
            NotificationType.RequestRequiresCompletion => "wnioski do uzupełnienia",
            NotificationType.ApprovalOverdue => "przeterminowane zatwierdzenia",
            NotificationType.VacationExpiringSoon => "wygasające urlopy",
            NotificationType.VacationAllowanceUpdated => "aktualizacje urlopów",
            NotificationType.RequestEdited => "edytowane wnioski",
            NotificationType.VacationCancelled => "anulowane urlopy",
            NotificationType.System => "systemowe",
            NotificationType.Announcement => "ogłoszenia",
            _ => "różne"
        };
    }

    private (DateTime start, DateTime end) GetDigestPeriod(DigestType type)
    {
        var now = DateTime.UtcNow;
        
        return type switch
        {
            DigestType.Daily => (now.Date.AddDays(-1), now.Date),
            DigestType.Weekly => (now.Date.AddDays(-7), now.Date),
            _ => (now.Date.AddDays(-1), now.Date)
        };
    }

    private string GenerateDigestSummary(List<NotificationGroupDto> groups, int totalNotifications)
    {
        if (totalNotifications == 0)
        {
            return "Brak nowych powiadomień w tym okresie.";
        }

        var summary = $"Masz {totalNotifications} powiadomień";
        
        if (groups.Count > 1)
        {
            summary += $" w {groups.Count} kategoriach";
        }
        
        summary += ".";

        // Add top notification types
        var topTypes = groups
            .OrderByDescending(g => g.Count)
            .Take(3)
            .Select(g => $"{g.Count} {GetNotificationTypeDisplayName(g.Type)}")
            .ToList();

        if (topTypes.Count > 0)
        {
            summary += $" Najczęstsze: {string.Join(", ", topTypes)}.";
        }

        return summary;
    }

    private async Task SendDigestEmailAsync(Guid userId, DigestNotificationDto digest)
    {
        try
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.Email))
            {
                return;
            }

            var subject = digest.Type == DigestType.Daily 
                ? "Podsumowanie powiadomień - dzisiaj" 
                : "Podsumowanie powiadomień - ten tydzień";

            // TODO: Create a proper email template for digest notifications
            // For now, send a simple summary
            await _emailService.SendNotificationEmailAsync(
                toEmail: user.Email,
                toName: user.FullName,
                notificationTitle: subject,
                notificationMessage: digest.Summary,
                actionUrl: "/dashboard/notifications",
                actionButtonText: "Zobacz wszystkie powiadomienia"
            );
        }
        catch (Exception)
        {
            // Don't let email failures break digest generation
        }
    }
}