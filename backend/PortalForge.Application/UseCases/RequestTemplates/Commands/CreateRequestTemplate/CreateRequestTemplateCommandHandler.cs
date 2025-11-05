using MediatR;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.UseCases.RequestTemplates.Commands.CreateRequestTemplate;

public class CreateRequestTemplateCommandHandler 
    : IRequestHandler<CreateRequestTemplateCommand, CreateRequestTemplateResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateRequestTemplateCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateRequestTemplateResult> Handle(
        CreateRequestTemplateCommand request, 
        CancellationToken cancellationToken)
    {
        var template = new RequestTemplate
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Icon = request.Icon,
            Category = request.Category,
            DepartmentId = request.DepartmentId,
            IsActive = true,
            RequiresApproval = request.RequiresApproval,
            EstimatedProcessingDays = request.EstimatedProcessingDays,
            PassingScore = request.PassingScore,
            CreatedById = request.CreatedById,
            CreatedAt = DateTime.UtcNow
        };

        // Add fields
        foreach (var fieldDto in request.Fields)
        {
            var field = new RequestTemplateField
            {
                Id = Guid.NewGuid(),
                RequestTemplateId = template.Id,
                Label = fieldDto.Label,
                FieldType = Enum.Parse<FieldType>(fieldDto.FieldType),
                Placeholder = fieldDto.Placeholder,
                IsRequired = fieldDto.IsRequired,
                Options = fieldDto.Options,
                MinValue = fieldDto.MinValue,
                MaxValue = fieldDto.MaxValue,
                HelpText = fieldDto.HelpText,
                Order = fieldDto.Order
            };
            template.Fields.Add(field);
        }

        // Add approval step templates
        foreach (var stepDto in request.ApprovalStepTemplates)
        {
            var step = new RequestApprovalStepTemplate
            {
                Id = Guid.NewGuid(),
                RequestTemplateId = template.Id,
                StepOrder = stepDto.StepOrder,
                ApproverType = Enum.Parse<ApproverType>(stepDto.ApproverType),
                SpecificUserId = stepDto.SpecificUserId,
                SpecificDepartmentId = stepDto.SpecificDepartmentId,
                ApproverGroupId = stepDto.ApproverGroupId,
                RequiresQuiz = stepDto.RequiresQuiz,
                CreatedAt = DateTime.UtcNow
            };
            template.ApprovalStepTemplates.Add(step);
        }

        // Add quiz questions
        foreach (var questionDto in request.QuizQuestions)
        {
            var question = new QuizQuestion
            {
                Id = Guid.NewGuid(),
                RequestTemplateId = template.Id,
                Question = questionDto.Question,
                Options = questionDto.Options,
                Order = questionDto.Order
            };
            template.QuizQuestions.Add(question);
        }

        await _unitOfWork.RequestTemplateRepository.CreateAsync(template);
        await _unitOfWork.SaveChangesAsync();

        return new CreateRequestTemplateResult
        {
            Id = template.Id,
            Message = "Request template created successfully"
        };
    }
}

