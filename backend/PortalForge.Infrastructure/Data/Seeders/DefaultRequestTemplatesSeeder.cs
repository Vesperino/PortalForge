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

        // Get or create system user (first admin) as creator
        var systemUser = await _context.Users
            .Where(u => u.Role == UserRole.Admin)
            .FirstOrDefaultAsync();

        if (systemUser == null)
        {
            _logger.LogWarning("No admin user found for template creation. Skipping seeding.");
            return;
        }

        // Check if templates exist
        var existingVacationTemplate = await _context.RequestTemplates
            .FirstOrDefaultAsync(t => t.Name == "Wniosek urlopowy");

        var existingSickLeaveTemplate = await _context.RequestTemplates
            .FirstOrDefaultAsync(t => t.Name == "Zgłoszenie L4 (zwolnienie lekarskie)");

        var templatesCreated = 0;

        // Vacation template
        if (existingVacationTemplate == null)
        {
            var vacationTemplate = CreateVacationRequestTemplate(systemUser.Id);
            await _context.RequestTemplates.AddAsync(vacationTemplate);
            _logger.LogInformation("Created vacation request template");
            templatesCreated++;
        }
        else
        {
            UpdateVacationRequestTemplate(existingVacationTemplate, systemUser.Id);
            _logger.LogInformation("Updated existing vacation request template");
        }

        // Sick leave template
        if (existingSickLeaveTemplate == null)
        {
            var sickLeaveTemplate = CreateSickLeaveTemplate(systemUser.Id);
            await _context.RequestTemplates.AddAsync(sickLeaveTemplate);
            _logger.LogInformation("Created sick leave template");
            templatesCreated++;
        }
        else
        {
            UpdateSickLeaveTemplate(existingSickLeaveTemplate, systemUser.Id);
            _logger.LogInformation("Updated existing sick leave template");
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Default request templates seeding completed. Created/Updated templates.");
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

    private void UpdateVacationRequestTemplate(RequestTemplate template, Guid updatedById)
    {
        // Update basic properties
        template.Description = "Standardowy wniosek o urlop wypoczynkowy, na żądanie lub okolicznościowy zgodny z polskim prawem pracy";
        template.Icon = "calendar";
        template.Category = "Urlopy i absencje";
        template.RequiresApproval = true;
        template.RequiresSubstituteSelection = true;
        template.AllowsAttachments = false;
        template.IsVacationRequest = true;
        template.IsSickLeaveRequest = false;
        template.MaxRetrospectiveDays = null;
        template.EstimatedProcessingDays = 3;
        template.IsActive = true;

        // Check existing fields and steps
        var existingFields = _context.RequestTemplateFields
            .Where(f => f.RequestTemplateId == template.Id)
            .ToList();

        var existingSteps = _context.RequestApprovalStepTemplates
            .Where(s => s.RequestTemplateId == template.Id)
            .ToList();

        // Check if there are any active requests using this template
        var hasActiveRequests = false;
        if (existingSteps.Any())
        {
            var stepIds = existingSteps.Select(s => s.Id).ToList();
            hasActiveRequests = _context.RequestApprovalSteps
                .Any(ras => ras.RequestApprovalStepTemplateId.HasValue &&
                            stepIds.Contains(ras.RequestApprovalStepTemplateId.Value));
        }

        // CRITICAL FIX: If there are NO fields, always add them (even with active requests)
        // This handles the case where fields were lost/deleted but requests exist
        var needsFieldsRestoration = !existingFields.Any();

        // If there are active requests AND fields exist, skip update to preserve data
        if (hasActiveRequests && !needsFieldsRestoration)
        {
            _logger.LogInformation("Skipping fields and approval steps update for vacation template - active requests exist and fields are present");
            return;
        }

        if (needsFieldsRestoration)
        {
            _logger.LogWarning("RESTORING MISSING FIELDS for vacation template (fields count: {Count})", existingFields.Count);
        }

        // Remove existing fields to prevent duplicates (only if they exist)
        if (existingFields.Any())
        {
            _context.RequestTemplateFields.RemoveRange(existingFields);
        }

        // Only remove/recreate approval steps if there are no active requests
        if (!hasActiveRequests && existingSteps.Any())
        {
            _context.RequestApprovalStepTemplates.RemoveRange(existingSteps);
        }

        // Add NEW fields (after removing old ones)
        var newFields = new List<RequestTemplateField>
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

        // Add to context as new entities
        _context.RequestTemplateFields.AddRange(newFields);

        // Only add NEW approval steps if there are no active requests
        if (!hasActiveRequests)
        {
            var newSteps = new List<RequestApprovalStepTemplate>
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

            // Add to context as new entities
            _context.RequestApprovalStepTemplates.AddRange(newSteps);
        }
        else
        {
            _logger.LogInformation("Keeping existing approval steps for vacation template - active requests exist");
        }
    }

    private void UpdateSickLeaveTemplate(RequestTemplate template, Guid updatedById)
    {
        // Update basic properties
        template.Description = "Zgłoszenie zwolnienia lekarskiego - automatycznie zatwierdzone zgodnie z prawem pracy, do wiadomości przełożonego";
        template.Icon = "medical-bag";
        template.Category = "Urlopy i absencje";
        template.RequiresApproval = true;
        template.RequiresSubstituteSelection = false;
        template.AllowsAttachments = true;
        template.IsVacationRequest = false;
        template.IsSickLeaveRequest = true;
        template.MaxRetrospectiveDays = 14;
        template.EstimatedProcessingDays = 0;
        template.IsActive = true;

        // Check existing fields and steps
        var existingFields = _context.RequestTemplateFields
            .Where(f => f.RequestTemplateId == template.Id)
            .ToList();

        var existingSteps = _context.RequestApprovalStepTemplates
            .Where(s => s.RequestTemplateId == template.Id)
            .ToList();

        // Check if there are any active requests using this template
        var hasActiveRequests = false;
        if (existingSteps.Any())
        {
            var stepIds = existingSteps.Select(s => s.Id).ToList();
            hasActiveRequests = _context.RequestApprovalSteps
                .Any(ras => ras.RequestApprovalStepTemplateId.HasValue &&
                            stepIds.Contains(ras.RequestApprovalStepTemplateId.Value));
        }

        // CRITICAL FIX: If there are NO fields, always add them (even with active requests)
        // This handles the case where fields were lost/deleted but requests exist
        var needsFieldsRestoration = !existingFields.Any();

        // If there are active requests AND fields exist, skip update to preserve data
        if (hasActiveRequests && !needsFieldsRestoration)
        {
            _logger.LogInformation("Skipping fields and approval steps update for sick leave template - active requests exist and fields are present");
            return;
        }

        if (needsFieldsRestoration)
        {
            _logger.LogWarning("RESTORING MISSING FIELDS for sick leave template (fields count: {Count})", existingFields.Count);
        }

        // Remove existing fields to prevent duplicates (only if they exist)
        if (existingFields.Any())
        {
            _context.RequestTemplateFields.RemoveRange(existingFields);
        }

        // Only remove/recreate approval steps if there are no active requests
        if (!hasActiveRequests && existingSteps.Any())
        {
            _context.RequestApprovalStepTemplates.RemoveRange(existingSteps);
        }

        // Add NEW fields (after removing old ones)
        var newFields = new List<RequestTemplateField>
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

        // Add to context as new entities
        _context.RequestTemplateFields.AddRange(newFields);

        // Only add NEW approval steps if there are no active requests
        if (!hasActiveRequests)
        {
            var newSteps = new List<RequestApprovalStepTemplate>
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

            // Add to context as new entities
            _context.RequestApprovalStepTemplates.AddRange(newSteps);
        }
        else
        {
            _logger.LogInformation("Keeping existing approval steps for sick leave template - active requests exist");
        }
    }
}
