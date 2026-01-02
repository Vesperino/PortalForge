using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Infrastructure.Data.Seeders.RequestTemplates;

/// <summary>
/// Seeder for HR-related request templates (training leave, special leave, etc.).
/// Note: Standard vacation and sick leave templates are handled by DefaultRequestTemplatesSeeder.
/// </summary>
public sealed class HRTemplateSeeder : IRequestTemplateSeeder
{
    public string Category => "HR";

    public Task<IReadOnlyList<RequestTemplate>> SeedAsync(
        Guid creatorId,
        CancellationToken cancellationToken = default)
    {
        var templates = new List<RequestTemplate>
        {
            CreateTrainingLeaveTemplate(creatorId)
        };

        return Task.FromResult<IReadOnlyList<RequestTemplate>>(templates);
    }

    private static RequestTemplate CreateTrainingLeaveTemplate(Guid creatorId)
    {
        var template = new RequestTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Urlop szkoleniowy",
            Description = "Wniosek o urlop szkoleniowy lub okolicznościowy",
            Icon = "Calendar",
            Category = "HR",
            DepartmentId = null,
            IsActive = true,
            RequiresApproval = true,
            EstimatedProcessingDays = 3,
            CreatedById = creatorId,
            CreatedAt = DateTime.UtcNow
        };

        template.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = template.Id,
            Label = "Rodzaj urlopu",
            FieldType = FieldType.Select,
            IsRequired = true,
            Options = "[{\"value\":\"training\",\"label\":\"Urlop szkoleniowy\"},{\"value\":\"occasional\",\"label\":\"Urlop okolicznościowy\"},{\"value\":\"unpaid\",\"label\":\"Urlop bezpłatny\"}]",
            Order = 1
        });

        template.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = template.Id,
            Label = "Data rozpoczęcia",
            FieldType = FieldType.Date,
            IsRequired = true,
            Order = 2
        });

        template.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = template.Id,
            Label = "Data zakończenia",
            FieldType = FieldType.Date,
            IsRequired = true,
            Order = 3
        });

        template.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = template.Id,
            Label = "Uzasadnienie",
            FieldType = FieldType.Textarea,
            IsRequired = true,
            Order = 4
        });

        template.ApprovalStepTemplates.Add(new RequestApprovalStepTemplate
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = template.Id,
            StepOrder = 1,
            ApproverType = ApproverType.DirectSupervisor,
            RequiresQuiz = false,
            CreatedAt = DateTime.UtcNow
        });

        return template;
    }
}
