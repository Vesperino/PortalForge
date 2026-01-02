using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Infrastructure.Data.Seeders.RequestTemplates;

/// <summary>
/// Seeder for security access request templates with security quiz.
/// </summary>
public sealed class SecurityAccessTemplateSeeder : IRequestTemplateSeeder
{
    public string Category => "Security";

    public Task<IReadOnlyList<RequestTemplate>> SeedAsync(
        Guid creatorId,
        CancellationToken cancellationToken = default)
    {
        var templates = new List<RequestTemplate>
        {
            CreateRDAccessTemplate(creatorId)
        };

        return Task.FromResult<IReadOnlyList<RequestTemplate>>(templates);
    }

    private static RequestTemplate CreateRDAccessTemplate(Guid creatorId)
    {
        var template = new RequestTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Dostęp do systemów R&D",
            Description = "Wniosek o nadanie dostępu do systemów badawczo-rozwojowych",
            Icon = "Shield",
            Category = "Security",
            DepartmentId = "IT",
            IsActive = true,
            RequiresApproval = true,
            EstimatedProcessingDays = 5,
            PassingScore = 80,
            CreatedById = creatorId,
            CreatedAt = DateTime.UtcNow
        };

        template.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = template.Id,
            Label = "System do którego potrzebujesz dostępu",
            FieldType = FieldType.Text,
            IsRequired = true,
            Order = 1
        });

        template.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = template.Id,
            Label = "Poziom dostępu",
            FieldType = FieldType.Select,
            IsRequired = true,
            Options = "[{\"value\":\"read\",\"label\":\"Tylko odczyt\"},{\"value\":\"write\",\"label\":\"Odczyt i zapis\"},{\"value\":\"admin\",\"label\":\"Administrator\"}]",
            Order = 2
        });

        template.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = template.Id,
            Label = "Uzasadnienie biznesowe",
            FieldType = FieldType.Textarea,
            IsRequired = true,
            Order = 3
        });

        var approvalStep = new RequestApprovalStepTemplate
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = template.Id,
            StepOrder = 1,
            ApproverType = ApproverType.DirectSupervisor,
            RequiresQuiz = true,
            CreatedAt = DateTime.UtcNow
        };

        approvalStep.QuizQuestions.Add(new QuizQuestion
        {
            Id = Guid.NewGuid(),
            RequestApprovalStepTemplateId = approvalStep.Id,
            Question = "Jak często należy zmieniać hasło do systemów R&D?",
            Options = "[{\"value\":\"a\",\"label\":\"Co 90 dni\",\"isCorrect\":true},{\"value\":\"b\",\"label\":\"Co roku\",\"isCorrect\":false},{\"value\":\"c\",\"label\":\"Nigdy\",\"isCorrect\":false}]",
            Order = 1
        });

        approvalStep.QuizQuestions.Add(new QuizQuestion
        {
            Id = Guid.NewGuid(),
            RequestApprovalStepTemplateId = approvalStep.Id,
            Question = "Czy można udostępniać dane z systemów R&D osobom trzecim?",
            Options = "[{\"value\":\"a\",\"label\":\"Nie, są poufne\",\"isCorrect\":true},{\"value\":\"b\",\"label\":\"Tak, zawsze\",\"isCorrect\":false},{\"value\":\"c\",\"label\":\"Tylko znajomym\",\"isCorrect\":false}]",
            Order = 2
        });

        approvalStep.QuizQuestions.Add(new QuizQuestion
        {
            Id = Guid.NewGuid(),
            RequestApprovalStepTemplateId = approvalStep.Id,
            Question = "Co zrobić w przypadku podejrzenia naruszenia bezpieczeństwa?",
            Options = "[{\"value\":\"a\",\"label\":\"Natychmiast zgłosić do działu IT\",\"isCorrect\":true},{\"value\":\"b\",\"label\":\"Zignorować\",\"isCorrect\":false},{\"value\":\"c\",\"label\":\"Powiedzieć kolegom\",\"isCorrect\":false}]",
            Order = 3
        });

        template.ApprovalStepTemplates.Add(approvalStep);

        return template;
    }
}
