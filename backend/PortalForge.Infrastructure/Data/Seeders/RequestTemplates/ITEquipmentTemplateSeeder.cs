using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Infrastructure.Data.Seeders.RequestTemplates;

/// <summary>
/// Seeder for IT equipment request templates.
/// </summary>
public sealed class ITEquipmentTemplateSeeder : IRequestTemplateSeeder
{
    public string Category => "Hardware";

    public Task<IReadOnlyList<RequestTemplate>> SeedAsync(
        Guid creatorId,
        CancellationToken cancellationToken = default)
    {
        var templates = new List<RequestTemplate>
        {
            CreateITEquipmentTemplate(creatorId)
        };

        return Task.FromResult<IReadOnlyList<RequestTemplate>>(templates);
    }

    private static RequestTemplate CreateITEquipmentTemplate(Guid creatorId)
    {
        var template = new RequestTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Zamówienie sprzętu IT",
            Description = "Wniosek o przydzielenie sprzętu komputerowego: laptop, monitor, akcesoria",
            Icon = "Laptop",
            Category = "Hardware",
            DepartmentId = "IT",
            IsActive = true,
            RequiresApproval = true,
            EstimatedProcessingDays = 7,
            CreatedById = creatorId,
            CreatedAt = DateTime.UtcNow
        };

        template.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = template.Id,
            Label = "Rodzaj sprzętu",
            FieldType = FieldType.Select,
            IsRequired = true,
            Options = "[{\"value\":\"laptop\",\"label\":\"Laptop\"},{\"value\":\"desktop\",\"label\":\"Komputer stacjonarny\"},{\"value\":\"monitor\",\"label\":\"Monitor\"},{\"value\":\"accessories\",\"label\":\"Akcesoria\"}]",
            Order = 1
        });

        template.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = template.Id,
            Label = "Uzasadnienie",
            FieldType = FieldType.Textarea,
            IsRequired = true,
            Placeholder = "Opisz, dlaczego potrzebujesz tego sprzętu...",
            Order = 2
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
