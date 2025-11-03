using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;
using PortalForge.Infrastructure.Persistence;

namespace PortalForge.Infrastructure.Data.Seeders;

/// <summary>
/// Seeder for default request templates (vacation request and sick leave).
/// Creates standard templates if they don't already exist.
/// </summary>
public class DefaultRequestTemplatesSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DefaultRequestTemplatesSeeder> _logger;

    public DefaultRequestTemplatesSeeder(
        ApplicationDbContext context,
        ILogger<DefaultRequestTemplatesSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Seeds default request templates if they don't exist.
    /// Idempotent - safe to run multiple times.
    /// </summary>
    public async Task SeedAsync()
    {
        _logger.LogInformation("Starting default request templates seeding");

        // Check if templates already exist
        var vacationTemplateExists = await _context.RequestTemplates
            .AnyAsync(t => t.Name == "Wniosek urlopowy");

        var sickLeaveTemplateExists = await _context.RequestTemplates
            .AnyAsync(t => t.Name == "Zgłoszenie L4 (zwolnienie lekarskie)");

        if (vacationTemplateExists && sickLeaveTemplateExists)
        {
            _logger.LogInformation("Default templates already exist. Skipping seeding.");
            return;
        }

        // Get system user (first admin) as creator
        var systemUser = await _context.Users
            .Where(u => u.Role == UserRole.Admin)
            .FirstOrDefaultAsync();

        if (systemUser == null)
        {
            _logger.LogWarning("No admin user found for template creation. Skipping seeding.");
            return;
        }

        var templates = new List<RequestTemplate>();

        if (!vacationTemplateExists)
        {
            templates.Add(CreateVacationRequestTemplate(systemUser.Id));
            _logger.LogInformation("Created vacation request template");
        }

        if (!sickLeaveTemplateExists)
        {
            templates.Add(CreateSickLeaveTemplate(systemUser.Id));
            _logger.LogInformation("Created sick leave template");
        }

        await _context.RequestTemplates.AddRangeAsync(templates);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Default request templates seeding completed. Created {Count} templates.", templates.Count);
    }

    private RequestTemplate CreateVacationRequestTemplate(Guid createdById)
    {
        var template = new RequestTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Wniosek urlopowy",
            Description = "Standardowy wniosek o urlop wypoczynkowy, na żądanie lub okolicznościowy zgodny z polskim prawem pracy",
            Icon = "calendar",
            Category = "Urlopy i absencje",
            RequiresApproval = true,
            RequiresSubstituteSelection = true,
            AllowsAttachments = false,
            IsVacationRequest = true,
            IsSickLeaveRequest = false,
            MaxRetrospectiveDays = null, // urlopy nie mogą być składane wstecz
            EstimatedProcessingDays = 3,
            IsActive = true,
            CreatedById = createdById,
            CreatedAt = DateTime.UtcNow
        };

        // Pola formularza
        template.Fields = new List<RequestTemplateField>
        {
            new RequestTemplateField
            {
                Id = Guid.NewGuid(),
                RequestTemplateId = template.Id,
                Label = "Typ urlopu",
                FieldType = FieldType.Select,
                IsRequired = true,
                Options = JsonSerializer.Serialize(new[]
                {
                    new { value = "Annual", label = "Urlop wypoczynkowy" },
                    new { value = "OnDemand", label = "Urlop na żądanie (max 4 dni/rok)" },
                    new { value = "Circumstantial", label = "Urlop okolicznościowy (2 dni)" }
                }),
                Order = 1,
                HelpText = "Urlop okolicznościowy: ślub, pogrzeb bliskiego, narodziny dziecka (2 dni). Urlop na żądanie: bez wcześniejszego wniosku, składany w dniu urlopu (max 4 dni/rok)"
            },
            new RequestTemplateField
            {
                Id = Guid.NewGuid(),
                RequestTemplateId = template.Id,
                Label = "Data rozpoczęcia",
                FieldType = FieldType.Date,
                IsRequired = true,
                Order = 2,
                HelpText = "Pierwszy dzień urlopu"
            },
            new RequestTemplateField
            {
                Id = Guid.NewGuid(),
                RequestTemplateId = template.Id,
                Label = "Data zakończenia",
                FieldType = FieldType.Date,
                IsRequired = true,
                Order = 3,
                HelpText = "Ostatni dzień urlopu (włącznie)"
            },
            new RequestTemplateField
            {
                Id = Guid.NewGuid(),
                RequestTemplateId = template.Id,
                Label = "Powód (dla urlopu okolicznościowego)",
                FieldType = FieldType.Textarea,
                IsRequired = false,
                Order = 4,
                Placeholder = "Np. ślub własny, pogrzeb bliskiego, narodziny dziecka",
                HelpText = "Pole wymagane dla urlopu okolicznościowego"
            }
        };

        // Approval flow: bezpośredni przełożony
        template.ApprovalStepTemplates = new List<RequestApprovalStepTemplate>
        {
            new RequestApprovalStepTemplate
            {
                Id = Guid.NewGuid(),
                RequestTemplateId = template.Id,
                StepOrder = 1,
                ApproverType = ApproverType.DirectSupervisor,
                RequiresQuiz = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        return template;
    }

    private RequestTemplate CreateSickLeaveTemplate(Guid createdById)
    {
        var template = new RequestTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Zgłoszenie L4 (zwolnienie lekarskie)",
            Description = "Zgłoszenie zwolnienia lekarskiego - automatycznie zatwierdzone zgodnie z prawem pracy, do wiadomości przełożonego",
            Icon = "medical-bag",
            Category = "Urlopy i absencje",
            RequiresApproval = true, // Do wiadomości przełożonego, ale auto-approve
            RequiresSubstituteSelection = false,
            AllowsAttachments = true, // Zaświadczenie ZUS po 33 dniach
            IsVacationRequest = false,
            IsSickLeaveRequest = true,
            MaxRetrospectiveDays = 14, // można zgłosić do 14 dni wstecz
            EstimatedProcessingDays = 0, // auto-approve
            IsActive = true,
            CreatedById = createdById,
            CreatedAt = DateTime.UtcNow
        };

        // Pola formularza
        template.Fields = new List<RequestTemplateField>
        {
            new RequestTemplateField
            {
                Id = Guid.NewGuid(),
                RequestTemplateId = template.Id,
                Label = "Data rozpoczęcia zwolnienia",
                FieldType = FieldType.Date,
                IsRequired = true,
                Order = 1,
                HelpText = "Możesz zgłosić zwolnienie do 14 dni wstecz"
            },
            new RequestTemplateField
            {
                Id = Guid.NewGuid(),
                RequestTemplateId = template.Id,
                Label = "Data zakończenia zwolnienia",
                FieldType = FieldType.Date,
                IsRequired = true,
                Order = 2,
                HelpText = "Przewidywany ostatni dzień zwolnienia (możesz zaktualizować później)"
            },
            new RequestTemplateField
            {
                Id = Guid.NewGuid(),
                RequestTemplateId = template.Id,
                Label = "Uwagi",
                FieldType = FieldType.Textarea,
                IsRequired = false,
                Order = 3,
                Placeholder = "Dodatkowe informacje dotyczące zwolnienia",
                HelpText = "Opcjonalne uwagi. Jeśli zwolnienie przekracza 33 dni, wymagane będzie zaświadczenie ZUS."
            }
        };

        // Approval flow: bezpośredni przełożony (ale auto-approve w kodzie SubmitRequestCommand)
        template.ApprovalStepTemplates = new List<RequestApprovalStepTemplate>
        {
            new RequestApprovalStepTemplate
            {
                Id = Guid.NewGuid(),
                RequestTemplateId = template.Id,
                StepOrder = 1,
                ApproverType = ApproverType.DirectSupervisor,
                RequiresQuiz = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        return template;
    }
}
