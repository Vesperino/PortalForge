using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.UseCases.RequestTemplates.Commands.UpdateRequestTemplate;

public class UpdateRequestTemplateCommandHandler 
    : IRequestHandler<UpdateRequestTemplateCommand, UpdateRequestTemplateResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateRequestTemplateCommandHandler> _logger;

    public UpdateRequestTemplateCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdateRequestTemplateCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UpdateRequestTemplateResult> Handle(
        UpdateRequestTemplateCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating request template {TemplateId}", request.Id);

        var template = await _unitOfWork.RequestTemplateRepository.GetByIdAsync(request.Id);
        
        if (template == null)
        {
            _logger.LogWarning("Request template {TemplateId} not found", request.Id);
            return new UpdateRequestTemplateResult
            {
                Success = false,
                Message = "Template not found"
            };
        }

        // Update only provided fields
        if (request.Name != null)
            template.Name = request.Name;
        
        if (request.Description != null)
            template.Description = request.Description;
        
        if (request.Icon != null)
            template.Icon = request.Icon;
        
        if (request.Category != null)
            template.Category = request.Category;

        if (request.DepartmentId != null)
            template.DepartmentId = request.DepartmentId;

        if (request.RequiresApproval.HasValue)
            template.RequiresApproval = request.RequiresApproval.Value;
        
        if (request.EstimatedProcessingDays.HasValue)
            template.EstimatedProcessingDays = request.EstimatedProcessingDays;

        if (request.PassingScore.HasValue)
            template.PassingScore = request.PassingScore;
        
        if (request.IsActive.HasValue)
            template.IsActive = request.IsActive.Value;

        // Update fields if provided
        if (request.Fields != null)
        {
            // Remove existing fields
            template.Fields.Clear();

            // Add new fields
            foreach (var fieldDto in request.Fields)
            {
                var field = new RequestTemplateField
                {
                    Id = fieldDto.Id != Guid.Empty ? fieldDto.Id : Guid.NewGuid(),
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
        }

        // Update approval steps if provided
        if (request.ApprovalStepTemplates != null)
        {
            // Remove existing approval steps
            template.ApprovalStepTemplates.Clear();

            // Add new approval steps
            foreach (var stepDto in request.ApprovalStepTemplates)
            {
                var step = new RequestApprovalStepTemplate
                {
                    Id = stepDto.Id != Guid.Empty ? stepDto.Id : Guid.NewGuid(),
                    RequestTemplateId = template.Id,
                    StepOrder = stepDto.StepOrder,
                    ApproverType = Enum.Parse<ApproverType>(stepDto.ApproverType),
                    ApproverRole = !string.IsNullOrEmpty(stepDto.ApproverRole)
                        ? Enum.Parse<DepartmentRole>(stepDto.ApproverRole)
                        : null,
                    SpecificUserId = stepDto.SpecificUserId,
                    ApproverGroupId = stepDto.ApproverGroupId,
                    RequiresQuiz = stepDto.RequiresQuiz,
                    CreatedAt = DateTime.UtcNow
                };
                template.ApprovalStepTemplates.Add(step);
            }
        }

        // Update quiz questions if provided
        if (request.QuizQuestions != null)
        {
            // Remove existing quiz questions
            template.QuizQuestions.Clear();

            // Add new quiz questions
            foreach (var questionDto in request.QuizQuestions)
            {
                var question = new QuizQuestion
                {
                    Id = questionDto.Id != Guid.Empty ? questionDto.Id : Guid.NewGuid(),
                    RequestTemplateId = template.Id,
                    Question = questionDto.Question,
                    Options = questionDto.Options,
                    Order = questionDto.Order
                };
                template.QuizQuestions.Add(question);
            }
        }

        template.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.RequestTemplateRepository.UpdateAsync(template);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Request template {TemplateId} updated successfully", request.Id);

        return new UpdateRequestTemplateResult
        {
            Success = true,
            Message = "Template updated successfully"
        };
    }
}

