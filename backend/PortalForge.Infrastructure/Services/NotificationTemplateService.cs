using System.Text.RegularExpressions;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Infrastructure.Services;

/// <summary>
/// Service for managing notification templates and rendering notifications.
/// </summary>
public class NotificationTemplateService : INotificationTemplateService
{
    private readonly IUnitOfWork _unitOfWork;
    private static readonly Regex PlaceholderRegex = new(@"\{(\w+)\}", RegexOptions.Compiled);

    public NotificationTemplateService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<(string title, string message)> RenderNotificationAsync(
        NotificationType type, 
        Dictionary<string, object> placeholders, 
        string language = "pl")
    {
        var template = await GetOrCreateDefaultTemplateAsync(type, language);
        
        var title = ReplacePlaceholders(template.TitleTemplate, placeholders);
        var message = ReplacePlaceholders(template.MessageTemplate, placeholders);
        
        return (title, message);
    }

    public async Task<(string subject, string body)> RenderEmailNotificationAsync(
        NotificationType type, 
        Dictionary<string, object> placeholders, 
        string language = "pl")
    {
        var template = await GetOrCreateDefaultTemplateAsync(type, language);
        
        var subject = !string.IsNullOrEmpty(template.EmailSubjectTemplate) 
            ? ReplacePlaceholders(template.EmailSubjectTemplate, placeholders)
            : ReplacePlaceholders(template.TitleTemplate, placeholders);
            
        var body = !string.IsNullOrEmpty(template.EmailBodyTemplate)
            ? ReplacePlaceholders(template.EmailBodyTemplate, placeholders)
            : ReplacePlaceholders(template.MessageTemplate, placeholders);
        
        return (subject, body);
    }

    public async Task<NotificationTemplate> GetOrCreateDefaultTemplateAsync(NotificationType type, string language = "pl")
    {
        var template = await _unitOfWork.NotificationTemplateRepository.GetByTypeAndLanguageAsync(type, language);
        
        if (template == null)
        {
            template = CreateDefaultTemplate(type, language);
            await _unitOfWork.NotificationTemplateRepository.CreateAsync(template);
            await _unitOfWork.SaveChangesAsync();
        }
        
        return template;
    }

    public async Task CreateDefaultTemplatesAsync(string language = "pl")
    {
        var notificationTypes = Enum.GetValues<NotificationType>();
        
        foreach (var type in notificationTypes)
        {
            var exists = await _unitOfWork.NotificationTemplateRepository.ExistsAsync(type, language);
            if (!exists)
            {
                var template = CreateDefaultTemplate(type, language);
                await _unitOfWork.NotificationTemplateRepository.CreateAsync(template);
            }
        }
        
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<List<string>> ValidateTemplateAsync(NotificationTemplate template, Dictionary<string, object> placeholders)
    {
        var errors = new List<string>();
        
        // Check title template
        var titlePlaceholders = ExtractPlaceholders(template.TitleTemplate);
        foreach (var placeholder in titlePlaceholders)
        {
            if (!placeholders.ContainsKey(placeholder))
            {
                errors.Add($"Title template contains undefined placeholder: {{{placeholder}}}");
            }
        }
        
        // Check message template
        var messagePlaceholders = ExtractPlaceholders(template.MessageTemplate);
        foreach (var placeholder in messagePlaceholders)
        {
            if (!placeholders.ContainsKey(placeholder))
            {
                errors.Add($"Message template contains undefined placeholder: {{{placeholder}}}");
            }
        }
        
        // Check email templates if they exist
        if (!string.IsNullOrEmpty(template.EmailSubjectTemplate))
        {
            var emailSubjectPlaceholders = ExtractPlaceholders(template.EmailSubjectTemplate);
            foreach (var placeholder in emailSubjectPlaceholders)
            {
                if (!placeholders.ContainsKey(placeholder))
                {
                    errors.Add($"Email subject template contains undefined placeholder: {{{placeholder}}}");
                }
            }
        }
        
        if (!string.IsNullOrEmpty(template.EmailBodyTemplate))
        {
            var emailBodyPlaceholders = ExtractPlaceholders(template.EmailBodyTemplate);
            foreach (var placeholder in emailBodyPlaceholders)
            {
                if (!placeholders.ContainsKey(placeholder))
                {
                    errors.Add($"Email body template contains undefined placeholder: {{{placeholder}}}");
                }
            }
        }
        
        return errors;
    }

    private string ReplacePlaceholders(string template, Dictionary<string, object> placeholders)
    {
        return PlaceholderRegex.Replace(template, match =>
        {
            var key = match.Groups[1].Value;
            return placeholders.TryGetValue(key, out var value) ? value?.ToString() ?? "" : match.Value;
        });
    }

    private List<string> ExtractPlaceholders(string template)
    {
        var matches = PlaceholderRegex.Matches(template);
        return matches.Select(m => m.Groups[1].Value).Distinct().ToList();
    }

    private NotificationTemplate CreateDefaultTemplate(NotificationType type, string language)
    {
        var (name, title, message, emailSubject, emailBody, placeholders) = GetDefaultTemplateContent(type, language);
        
        return new NotificationTemplate
        {
            Id = Guid.NewGuid(),
            Type = type,
            Name = name,
            TitleTemplate = title,
            MessageTemplate = message,
            EmailSubjectTemplate = emailSubject,
            EmailBodyTemplate = emailBody,
            IsActive = true,
            Language = language,
            PlaceholderDefinitions = placeholders,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private (string name, string title, string message, string? emailSubject, string? emailBody, string placeholders) 
        GetDefaultTemplateContent(NotificationType type, string language)
    {
        // For Polish language templates
        if (language == "pl")
        {
            return type switch
            {
                NotificationType.RequestPendingApproval => (
                    "Wniosek oczekuje na zatwierdzenie",
                    "Nowy wniosek do zatwierdzenia: {RequestType}",
                    "{SubmitterName} przesłał wniosek \"{RequestType}\" oczekujący na Twoje zatwierdzenie.",
                    "Nowy wniosek do zatwierdzenia: {RequestType}",
                    "Witaj {ApproverName},\n\n{SubmitterName} przesłał wniosek \"{RequestType}\" oczekujący na Twoje zatwierdzenie.\n\nKliknij poniższy link, aby przejrzeć wniosek:\n{ActionUrl}",
                    "{\"SubmitterName\": \"Imię i nazwisko składającego wniosek\", \"RequestType\": \"Typ wniosku\", \"ApproverName\": \"Imię i nazwisko zatwierdzającego\", \"ActionUrl\": \"Link do wniosku\"}"
                ),
                
                NotificationType.RequestApproved => (
                    "Wniosek zatwierdzony",
                    "Wniosek zatwierdzony: {RequestType}",
                    "Twój wniosek \"{RequestType}\" został zatwierdzony przez {ApproverName}.",
                    "Wniosek zatwierdzony: {RequestType}",
                    "Witaj {SubmitterName},\n\nTwój wniosek \"{RequestType}\" został zatwierdzony przez {ApproverName}.\n\nKliknij poniższy link, aby zobaczyć szczegóły:\n{ActionUrl}",
                    "{\"SubmitterName\": \"Imię i nazwisko składającego wniosek\", \"RequestType\": \"Typ wniosku\", \"ApproverName\": \"Imię i nazwisko zatwierdzającego\", \"ActionUrl\": \"Link do wniosku\"}"
                ),
                
                NotificationType.RequestRejected => (
                    "Wniosek odrzucony",
                    "Wniosek odrzucony: {RequestType}",
                    "Twój wniosek \"{RequestType}\" został odrzucony przez {ApproverName}. Powód: {Reason}",
                    "Wniosek odrzucony: {RequestType}",
                    "Witaj {SubmitterName},\n\nTwój wniosek \"{RequestType}\" został odrzucony przez {ApproverName}.\n\nPowód odrzucenia: {Reason}\n\nKliknij poniższy link, aby zobaczyć szczegóły:\n{ActionUrl}",
                    "{\"SubmitterName\": \"Imię i nazwisko składającego wniosek\", \"RequestType\": \"Typ wniosku\", \"ApproverName\": \"Imię i nazwisko zatwierdzającego\", \"Reason\": \"Powód odrzucenia\", \"ActionUrl\": \"Link do wniosku\"}"
                ),
                
                NotificationType.VacationCoverageAssigned => (
                    "Przypisano zastępstwo",
                    "Zostałeś zastępcą podczas urlopu",
                    "Będziesz zastępcą dla {EmployeeName} w okresie od {StartDate} do {EndDate}.",
                    "Przypisano Ci zastępstwo",
                    "Witaj {SubstituteName},\n\nZostałeś przypisany jako zastępca dla {EmployeeName} w okresie od {StartDate} do {EndDate}.\n\nKliknij poniższy link, aby zobaczyć szczegóły:\n{ActionUrl}",
                    "{\"SubstituteName\": \"Imię i nazwisko zastępcy\", \"EmployeeName\": \"Imię i nazwisko pracownika na urlopie\", \"StartDate\": \"Data rozpoczęcia\", \"EndDate\": \"Data zakończenia\", \"ActionUrl\": \"Link do zastępstw\"}"
                ),
                
                NotificationType.ApprovalOverdue => (
                    "Przeterminowane zatwierdzenie",
                    "⚠️ Wniosek wymaga pilnej akceptacji",
                    "Wniosek \"{RequestType}\" od {SubmitterName} oczekuje już {DaysOverdue} dni na Twoją akceptację. Proszę o szybkie rozpatrzenie.",
                    "⚠️ Przeterminowane zatwierdzenie: {RequestType}",
                    "Witaj {ApproverName},\n\nWniosek \"{RequestType}\" od {SubmitterName} oczekuje już {DaysOverdue} dni na Twoją akceptację.\n\nProszę o szybkie rozpatrzenie tego wniosku:\n{ActionUrl}",
                    "{\"ApproverName\": \"Imię i nazwisko zatwierdzającego\", \"RequestType\": \"Typ wniosku\", \"SubmitterName\": \"Imię i nazwisko składającego\", \"DaysOverdue\": \"Liczba dni opóźnienia\", \"ActionUrl\": \"Link do wniosku\"}"
                ),
                
                NotificationType.System => (
                    "Powiadomienie systemowe",
                    "{Title}",
                    "{Message}",
                    "{Title}",
                    "Witaj {UserName},\n\n{Message}\n\n{ActionUrl}",
                    "{\"UserName\": \"Imię i nazwisko użytkownika\", \"Title\": \"Tytuł powiadomienia\", \"Message\": \"Treść powiadomienia\", \"ActionUrl\": \"Link do akcji\"}"
                ),
                
                _ => (
                    $"Szablon dla {type}",
                    "{Title}",
                    "{Message}",
                    "{Title}",
                    "Witaj {UserName},\n\n{Message}\n\n{ActionUrl}",
                    "{\"UserName\": \"Imię i nazwisko użytkownika\", \"Title\": \"Tytuł powiadomienia\", \"Message\": \"Treść powiadomienia\", \"ActionUrl\": \"Link do akcji\"}"
                )
            };
        }
        
        // Default English templates
        return type switch
        {
            NotificationType.RequestPendingApproval => (
                "Request pending approval",
                "New request for approval: {RequestType}",
                "{SubmitterName} submitted a request \"{RequestType}\" waiting for your approval.",
                "New request for approval: {RequestType}",
                "Hello {ApproverName},\n\n{SubmitterName} submitted a request \"{RequestType}\" waiting for your approval.\n\nClick the link below to review the request:\n{ActionUrl}",
                "{\"SubmitterName\": \"Name of request submitter\", \"RequestType\": \"Type of request\", \"ApproverName\": \"Name of approver\", \"ActionUrl\": \"Link to request\"}"
            ),
            
            _ => (
                $"Template for {type}",
                "{Title}",
                "{Message}",
                "{Title}",
                "Hello {UserName},\n\n{Message}\n\n{ActionUrl}",
                "{\"UserName\": \"User name\", \"Title\": \"Notification title\", \"Message\": \"Notification message\", \"ActionUrl\": \"Action link\"}"
            )
        };
    }
}