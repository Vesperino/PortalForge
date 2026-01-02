using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Infrastructure.Data.Seeders.RequestTemplates;

/// <summary>
/// Seeder for training request templates with quiz functionality.
/// </summary>
public sealed class TrainingTemplateSeeder : IRequestTemplateSeeder
{
    public string Category => "Training";

    public Task<IReadOnlyList<RequestTemplate>> SeedAsync(
        Guid creatorId,
        CancellationToken cancellationToken = default)
    {
        var templates = new List<RequestTemplate>
        {
            CreateExternalTrainingTemplate(creatorId)
        };

        return Task.FromResult<IReadOnlyList<RequestTemplate>>(templates);
    }

    private static RequestTemplate CreateExternalTrainingTemplate(Guid creatorId)
    {
        var template = new RequestTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Szkolenie zewnętrzne",
            Description = "Wniosek o udział w szkoleniu lub konferencji zewnętrznej",
            Icon = "GraduationCap",
            Category = "Training",
            DepartmentId = null,
            IsActive = true,
            RequiresApproval = true,
            EstimatedProcessingDays = 14,
            PassingScore = 80,
            CreatedById = creatorId,
            CreatedAt = DateTime.UtcNow
        };

        template.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = template.Id,
            Label = "Nazwa szkolenia",
            FieldType = FieldType.Text,
            IsRequired = true,
            Order = 1
        });

        template.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = template.Id,
            Label = "Organizator",
            FieldType = FieldType.Text,
            IsRequired = true,
            Order = 2
        });

        template.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = template.Id,
            Label = "Data szkolenia",
            FieldType = FieldType.Date,
            IsRequired = true,
            Order = 3
        });

        template.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = template.Id,
            Label = "Koszt (PLN)",
            FieldType = FieldType.Number,
            IsRequired = true,
            MinValue = 0,
            Order = 4
        });

        template.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = template.Id,
            Label = "Cel i korzyści",
            FieldType = FieldType.Textarea,
            IsRequired = true,
            Placeholder = "Opisz, jak to szkolenie przyczyni się do rozwoju i pracy...",
            Order = 5
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
            Question = "Jakie są obowiązki pracownika po ukończeniu szkolenia?",
            Options = "[{\"value\":\"a\",\"label\":\"Podzielenie się wiedzą z zespołem\",\"isCorrect\":true},{\"value\":\"b\",\"label\":\"Brak obowiązków\",\"isCorrect\":false},{\"value\":\"c\",\"label\":\"Tylko wykonywanie swojej pracy\",\"isCorrect\":false}]",
            Order = 1
        });

        approvalStep.QuizQuestions.Add(new QuizQuestion
        {
            Id = Guid.NewGuid(),
            RequestApprovalStepTemplateId = approvalStep.Id,
            Question = "W jakim terminie należy przedstawić raport z szkolenia?",
            Options = "[{\"value\":\"a\",\"label\":\"W ciągu 7 dni roboczych\",\"isCorrect\":true},{\"value\":\"b\",\"label\":\"Nie ma wymogu\",\"isCorrect\":false},{\"value\":\"c\",\"label\":\"W ciągu miesiąca\",\"isCorrect\":false}]",
            Order = 2
        });

        template.ApprovalStepTemplates.Add(approvalStep);

        return template;
    }
}
