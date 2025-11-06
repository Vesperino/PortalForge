using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.UseCases.RequestTemplates.Commands.SeedRequestTemplates;

public class SeedRequestTemplatesCommandHandler 
    : IRequestHandler<SeedRequestTemplatesCommand, SeedRequestTemplatesResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SeedRequestTemplatesCommandHandler> _logger;

    public SeedRequestTemplatesCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<SeedRequestTemplatesCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<SeedRequestTemplatesResult> Handle(
        SeedRequestTemplatesCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting request templates seeding...");

        // Check if templates already exist
        var existingTemplates = await _unitOfWork.RequestTemplateRepository.GetAllAsync();
        if (existingTemplates.Any())
        {
            _logger.LogInformation("Request templates already exist. Skipping seed.");
            return new SeedRequestTemplatesResult
            {
                TemplatesCreated = 0,
                Message = "Templates already exist"
            };
        }

        // Get first admin user as creator
        var users = await _unitOfWork.UserRepository.GetAllAsync();
        var adminUser = users.FirstOrDefault(u => u.Role == UserRole.Admin);
        if (adminUser == null)
        {
            _logger.LogWarning("No admin user found for seeding templates");
            return new SeedRequestTemplatesResult
            {
                TemplatesCreated = 0,
                Message = "No admin user found"
            };
        }

        var count = 0;

        // Template 1: IT Equipment Request
        var itEquipmentTemplate = new RequestTemplate
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
            CreatedById = adminUser.Id,
            CreatedAt = DateTime.UtcNow
        };

        itEquipmentTemplate.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = itEquipmentTemplate.Id,
            Label = "Rodzaj sprzętu",
            FieldType = FieldType.Select,
            IsRequired = true,
            Options = "[{\"value\":\"laptop\",\"label\":\"Laptop\"},{\"value\":\"desktop\",\"label\":\"Komputer stacjonarny\"},{\"value\":\"monitor\",\"label\":\"Monitor\"},{\"value\":\"accessories\",\"label\":\"Akcesoria\"}]",
            Order = 1
        });

        itEquipmentTemplate.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = itEquipmentTemplate.Id,
            Label = "Uzasadnienie",
            FieldType = FieldType.Textarea,
            IsRequired = true,
            Placeholder = "Opisz, dlaczego potrzebujesz tego sprzętu...",
            Order = 2
        });

        itEquipmentTemplate.ApprovalStepTemplates.Add(new RequestApprovalStepTemplate
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = itEquipmentTemplate.Id,
            StepOrder = 1,
            ApproverType = ApproverType.DirectSupervisor,
            RequiresQuiz = false,
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.RequestTemplateRepository.CreateAsync(itEquipmentTemplate);
        count++;

        // Template 2: Training Request with Quiz
        var trainingTemplate = new RequestTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Szkolenie zewnętrzne",
            Description = "Wniosek o udział w szkoleniu lub konferencji zewnętrznej",
            Icon = "GraduationCap",
            Category = "Training",
            DepartmentId = null, // Available for all departments
            IsActive = true,
            RequiresApproval = true,
            EstimatedProcessingDays = 14,
            PassingScore = 80,
            CreatedById = adminUser.Id,
            CreatedAt = DateTime.UtcNow
        };

        trainingTemplate.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = trainingTemplate.Id,
            Label = "Nazwa szkolenia",
            FieldType = FieldType.Text,
            IsRequired = true,
            Order = 1
        });

        trainingTemplate.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = trainingTemplate.Id,
            Label = "Organizator",
            FieldType = FieldType.Text,
            IsRequired = true,
            Order = 2
        });

        trainingTemplate.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = trainingTemplate.Id,
            Label = "Data szkolenia",
            FieldType = FieldType.Date,
            IsRequired = true,
            Order = 3
        });

        trainingTemplate.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = trainingTemplate.Id,
            Label = "Koszt (PLN)",
            FieldType = FieldType.Number,
            IsRequired = true,
            MinValue = 0,
            Order = 4
        });

        trainingTemplate.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = trainingTemplate.Id,
            Label = "Cel i korzyści",
            FieldType = FieldType.Textarea,
            IsRequired = true,
            Placeholder = "Opisz, jak to szkolenie przyczyni się do rozwoju i pracy...",
            Order = 5
        });

        var trainingStep = new RequestApprovalStepTemplate
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = trainingTemplate.Id,
            StepOrder = 1,
            ApproverType = ApproverType.DirectSupervisor,
            RequiresQuiz = true,
            CreatedAt = DateTime.UtcNow
        };

        trainingStep.QuizQuestions.Add(new QuizQuestion
        {
            Id = Guid.NewGuid(),
            RequestApprovalStepTemplateId = trainingStep.Id,
            Question = "Jakie są obowiązki pracownika po ukończeniu szkolenia?",
            Options = "[{\"value\":\"a\",\"label\":\"Podzielenie się wiedzą z zespołem\",\"isCorrect\":true},{\"value\":\"b\",\"label\":\"Brak obowiązków\",\"isCorrect\":false},{\"value\":\"c\",\"label\":\"Tylko wykonywanie swojej pracy\",\"isCorrect\":false}]",
            Order = 1
        });

        trainingStep.QuizQuestions.Add(new QuizQuestion
        {
            Id = Guid.NewGuid(),
            RequestApprovalStepTemplateId = trainingStep.Id,
            Question = "W jakim terminie należy przedstawić raport z szkolenia?",
            Options = "[{\"value\":\"a\",\"label\":\"W ciągu 7 dni roboczych\",\"isCorrect\":true},{\"value\":\"b\",\"label\":\"Nie ma wymogu\",\"isCorrect\":false},{\"value\":\"c\",\"label\":\"W ciągu miesiąca\",\"isCorrect\":false}]",
            Order = 2
        });

        trainingTemplate.ApprovalStepTemplates.Add(trainingStep);

        await _unitOfWork.RequestTemplateRepository.CreateAsync(trainingTemplate);
        count++;

        // Template 3: R&D Access Request with Security Quiz
        var rdAccessTemplate = new RequestTemplate
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
            CreatedById = adminUser.Id,
            CreatedAt = DateTime.UtcNow
        };

        rdAccessTemplate.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = rdAccessTemplate.Id,
            Label = "System do którego potrzebujesz dostępu",
            FieldType = FieldType.Text,
            IsRequired = true,
            Order = 1
        });

        rdAccessTemplate.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = rdAccessTemplate.Id,
            Label = "Poziom dostępu",
            FieldType = FieldType.Select,
            IsRequired = true,
            Options = "[{\"value\":\"read\",\"label\":\"Tylko odczyt\"},{\"value\":\"write\",\"label\":\"Odczyt i zapis\"},{\"value\":\"admin\",\"label\":\"Administrator\"}]",
            Order = 2
        });

        rdAccessTemplate.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = rdAccessTemplate.Id,
            Label = "Uzasadnienie biznesowe",
            FieldType = FieldType.Textarea,
            IsRequired = true,
            Order = 3
        });

        var rdAccessStep = new RequestApprovalStepTemplate
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = rdAccessTemplate.Id,
            StepOrder = 1,
            ApproverType = ApproverType.DirectSupervisor,
            RequiresQuiz = true,
            CreatedAt = DateTime.UtcNow
        };

        rdAccessStep.QuizQuestions.Add(new QuizQuestion
        {
            Id = Guid.NewGuid(),
            RequestApprovalStepTemplateId = rdAccessStep.Id,
            Question = "Jak często należy zmieniać hasło do systemów R&D?",
            Options = "[{\"value\":\"a\",\"label\":\"Co 90 dni\",\"isCorrect\":true},{\"value\":\"b\",\"label\":\"Co roku\",\"isCorrect\":false},{\"value\":\"c\",\"label\":\"Nigdy\",\"isCorrect\":false}]",
            Order = 1
        });

        rdAccessStep.QuizQuestions.Add(new QuizQuestion
        {
            Id = Guid.NewGuid(),
            RequestApprovalStepTemplateId = rdAccessStep.Id,
            Question = "Czy można udostępniać dane z systemów R&D osobom trzecim?",
            Options = "[{\"value\":\"a\",\"label\":\"Nie, są poufne\",\"isCorrect\":true},{\"value\":\"b\",\"label\":\"Tak, zawsze\",\"isCorrect\":false},{\"value\":\"c\",\"label\":\"Tylko znajomym\",\"isCorrect\":false}]",
            Order = 2
        });

        rdAccessStep.QuizQuestions.Add(new QuizQuestion
        {
            Id = Guid.NewGuid(),
            RequestApprovalStepTemplateId = rdAccessStep.Id,
            Question = "Co zrobić w przypadku podejrzenia naruszenia bezpieczeństwa?",
            Options = "[{\"value\":\"a\",\"label\":\"Natychmiast zgłosić do działu IT\",\"isCorrect\":true},{\"value\":\"b\",\"label\":\"Zignorować\",\"isCorrect\":false},{\"value\":\"c\",\"label\":\"Powiedzieć kolegom\",\"isCorrect\":false}]",
            Order = 3
        });

        rdAccessTemplate.ApprovalStepTemplates.Add(rdAccessStep);

        await _unitOfWork.RequestTemplateRepository.CreateAsync(rdAccessTemplate);
        count++;

        // Template 4: Vacation Request (HR)
        var vacationTemplate = new RequestTemplate
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
            CreatedById = adminUser.Id,
            CreatedAt = DateTime.UtcNow
        };

        vacationTemplate.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = vacationTemplate.Id,
            Label = "Rodzaj urlopu",
            FieldType = FieldType.Select,
            IsRequired = true,
            Options = "[{\"value\":\"training\",\"label\":\"Urlop szkoleniowy\"},{\"value\":\"occasional\",\"label\":\"Urlop okolicznościowy\"},{\"value\":\"unpaid\",\"label\":\"Urlop bezpłatny\"}]",
            Order = 1
        });

        vacationTemplate.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = vacationTemplate.Id,
            Label = "Data rozpoczęcia",
            FieldType = FieldType.Date,
            IsRequired = true,
            Order = 2
        });

        vacationTemplate.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = vacationTemplate.Id,
            Label = "Data zakończenia",
            FieldType = FieldType.Date,
            IsRequired = true,
            Order = 3
        });

        vacationTemplate.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = vacationTemplate.Id,
            Label = "Uzasadnienie",
            FieldType = FieldType.Textarea,
            IsRequired = true,
            Order = 4
        });

        vacationTemplate.ApprovalStepTemplates.Add(new RequestApprovalStepTemplate
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = vacationTemplate.Id,
            StepOrder = 1,
            ApproverType = ApproverType.DirectSupervisor,
            RequiresQuiz = false,
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.RequestTemplateRepository.CreateAsync(vacationTemplate);
        count++;

        // Template 5: Software License Request
        var softwareLicenseTemplate = new RequestTemplate
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
            CreatedById = adminUser.Id,
            CreatedAt = DateTime.UtcNow
        };

        softwareLicenseTemplate.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = softwareLicenseTemplate.Id,
            Label = "Nazwa oprogramowania",
            FieldType = FieldType.Text,
            IsRequired = true,
            Order = 1
        });

        softwareLicenseTemplate.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = softwareLicenseTemplate.Id,
            Label = "Typ licencji",
            FieldType = FieldType.Select,
            IsRequired = true,
            Options = "[{\"value\":\"new\",\"label\":\"Nowa licencja\"},{\"value\":\"renewal\",\"label\":\"Przedłużenie\"},{\"value\":\"upgrade\",\"label\":\"Upgrade\"}]",
            Order = 2
        });

        softwareLicenseTemplate.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = softwareLicenseTemplate.Id,
            Label = "Szacowany koszt (PLN)",
            FieldType = FieldType.Number,
            IsRequired = true,
            MinValue = 0,
            Order = 3
        });

        softwareLicenseTemplate.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = softwareLicenseTemplate.Id,
            Label = "Cel wykorzystania",
            FieldType = FieldType.Textarea,
            IsRequired = true,
            Order = 4
        });

        softwareLicenseTemplate.ApprovalStepTemplates.Add(new RequestApprovalStepTemplate
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = softwareLicenseTemplate.Id,
            StepOrder = 1,
            ApproverType = ApproverType.DirectSupervisor,
            RequiresQuiz = false,
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.RequestTemplateRepository.CreateAsync(softwareLicenseTemplate);
        count++;

        await _unitOfWork.SaveChangesAsync();

        var message = $"Created {count} request templates";
        _logger.LogInformation(message);

        return new SeedRequestTemplatesResult
        {
            TemplatesCreated = count,
            Message = message
        };
    }
}

