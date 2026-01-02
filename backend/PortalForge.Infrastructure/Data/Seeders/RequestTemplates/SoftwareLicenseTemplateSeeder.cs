using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Infrastructure.Data.Seeders.RequestTemplates;

/// <summary>
/// Seeder for software license request templates.
/// </summary>
public sealed class SoftwareLicenseTemplateSeeder : IRequestTemplateSeeder
{
    public string Category => "Software";

    public Task<IReadOnlyList<RequestTemplate>> SeedAsync(
        Guid creatorId,
        CancellationToken cancellationToken = default)
    {
        var templates = new List<RequestTemplate>
        {
            CreateSoftwareLicenseTemplate(creatorId)
        };

        return Task.FromResult<IReadOnlyList<RequestTemplate>>(templates);
    }

    private static RequestTemplate CreateSoftwareLicenseTemplate(Guid creatorId)
    {
        var template = new RequestTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Licencja na oprogramowanie",
            Description = "Wniosek o zakup lub przedłużenie licencji na oprogramowanie",
            Icon = "Package",
            Category = "Software",
            DepartmentId = null,
            IsActive = true,
            RequiresApproval = true,
            EstimatedProcessingDays = 10,
            CreatedById = creatorId,
            CreatedAt = DateTime.UtcNow
        };

        template.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = template.Id,
            Label = "Nazwa oprogramowania",
            FieldType = FieldType.Text,
            IsRequired = true,
            Order = 1
        });

        template.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = template.Id,
            Label = "Typ licencji",
            FieldType = FieldType.Select,
            IsRequired = true,
            Options = "[{\"value\":\"new\",\"label\":\"Nowa licencja\"},{\"value\":\"renewal\",\"label\":\"Przedłużenie\"},{\"value\":\"upgrade\",\"label\":\"Upgrade\"}]",
            Order = 2
        });

        template.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = template.Id,
            Label = "Szacowany koszt (PLN)",
            FieldType = FieldType.Number,
            IsRequired = true,
            MinValue = 0,
            Order = 3
        });

        template.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = template.Id,
            Label = "Cel wykorzystania",
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
