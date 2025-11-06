using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.RequestTemplates.DTOs;
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
            var requestFieldIds = request.Fields
                .Where(f => f.Id != Guid.Empty)
                .Select(f => f.Id)
                .ToList();

            // Remove fields that are no longer in the request
            var fieldsToRemove = template.Fields
                .Where(f => !requestFieldIds.Contains(f.Id))
                .ToList();

            foreach (var field in fieldsToRemove)
            {
                template.Fields.Remove(field);
            }

            // Update or add fields
            foreach (var fieldDto in request.Fields)
            {
                var existingField = template.Fields.FirstOrDefault(f => f.Id == fieldDto.Id && fieldDto.Id != Guid.Empty);

                if (existingField != null)
                {
                    // Update existing field
                    existingField.Label = fieldDto.Label;
                    existingField.FieldType = Enum.Parse<FieldType>(fieldDto.FieldType);
                    existingField.Placeholder = fieldDto.Placeholder;
                    existingField.IsRequired = fieldDto.IsRequired;
                    existingField.Options = fieldDto.Options;
                    existingField.MinValue = fieldDto.MinValue;
                    existingField.MaxValue = fieldDto.MaxValue;
                    existingField.HelpText = fieldDto.HelpText;
                    existingField.Order = fieldDto.Order;
                }
                else
                {
                    // Add new field
                    var newField = new RequestTemplateField
                    {
                        Id = Guid.NewGuid(),
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
                    template.Fields.Add(newField);
                }
            }
        }

        // Update approval steps if provided
        if (request.ApprovalStepTemplates != null)
        {
            var requestStepIds = request.ApprovalStepTemplates
                .Where(s => s.Id != Guid.Empty)
                .Select(s => s.Id)
                .ToList();

            // Remove steps that are no longer in the request
            var stepsToRemove = template.ApprovalStepTemplates
                .Where(s => !requestStepIds.Contains(s.Id))
                .ToList();

            foreach (var step in stepsToRemove)
            {
                template.ApprovalStepTemplates.Remove(step);
            }

            // Update or add approval steps
            foreach (var stepDto in request.ApprovalStepTemplates)
            {
                var existingStep = template.ApprovalStepTemplates.FirstOrDefault(s => s.Id == stepDto.Id && stepDto.Id != Guid.Empty);

                if (existingStep != null)
                {
                    // Update existing step
                    existingStep.StepOrder = stepDto.StepOrder;

                    // Parse and update ApproverType
                    var newApproverType = Enum.Parse<ApproverType>(stepDto.ApproverType);
                    existingStep.ApproverType = newApproverType;

                    existingStep.SpecificUserId = stepDto.SpecificUserId;
                    existingStep.SpecificDepartmentId = stepDto.SpecificDepartmentId;

                    // Update SpecificDepartmentRoleType only for SpecificDepartment approver type
                    if (newApproverType == ApproverType.SpecificDepartment)
                    {
                        if (!string.IsNullOrEmpty(stepDto.SpecificDepartmentRoleType))
                        {
                            existingStep.SpecificDepartmentRoleType = Enum.Parse<DepartmentRoleType>(stepDto.SpecificDepartmentRoleType);
                        }
                        else
                        {
                            existingStep.SpecificDepartmentRoleType = DepartmentRoleType.Head;
                        }
                    }
                    else
                    {
                        // Reset to default when not using SpecificDepartment
                        existingStep.SpecificDepartmentRoleType = DepartmentRoleType.Head;
                    }

                    existingStep.ApproverGroupId = stepDto.ApproverGroupId;
                    existingStep.RequiresQuiz = stepDto.RequiresQuiz;
                    existingStep.PassingScore = stepDto.PassingScore;

                    // Update quiz questions for this step
                    UpdateStepQuizQuestions(existingStep, stepDto.QuizQuestions);
                }
                else
                {
                    // Add new step
                    var newStep = new RequestApprovalStepTemplate
                    {
                        Id = Guid.NewGuid(),
                        StepOrder = stepDto.StepOrder,
                        ApproverType = Enum.Parse<ApproverType>(stepDto.ApproverType),
                        SpecificUserId = stepDto.SpecificUserId,
                        SpecificDepartmentId = stepDto.SpecificDepartmentId,
                        SpecificDepartmentRoleType = !string.IsNullOrEmpty(stepDto.SpecificDepartmentRoleType)
                            ? Enum.Parse<DepartmentRoleType>(stepDto.SpecificDepartmentRoleType)
                            : DepartmentRoleType.Head,
                        ApproverGroupId = stepDto.ApproverGroupId,
                        RequiresQuiz = stepDto.RequiresQuiz,
                        PassingScore = stepDto.PassingScore,
                        CreatedAt = DateTime.UtcNow
                    };

                    // Add quiz questions for new step
                    if (stepDto.QuizQuestions != null)
                    {
                        foreach (var questionDto in stepDto.QuizQuestions)
                        {
                            var newQuestion = new QuizQuestion
                            {
                                Id = Guid.NewGuid(),
                                Question = questionDto.Question,
                                Options = questionDto.Options,
                                Order = questionDto.Order
                            };
                            newStep.QuizQuestions.Add(newQuestion);
                        }
                    }

                    template.ApprovalStepTemplates.Add(newStep);
                }
            }
        }

        template.UpdatedAt = DateTime.UtcNow;

        // No need to call UpdateAsync - entity is already tracked by EF Change Tracker
        // Changes to collections and properties are automatically detected
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Request template {TemplateId} updated successfully", request.Id);

        return new UpdateRequestTemplateResult
        {
            Success = true,
            Message = "Template updated successfully"
        };
    }

    private void UpdateStepQuizQuestions(RequestApprovalStepTemplate step, List<QuizQuestionDto>? quizQuestions)
    {
        if (quizQuestions == null)
        {
            // If no quiz questions provided, clear existing ones
            step.QuizQuestions.Clear();
            return;
        }

        var requestQuestionIds = quizQuestions
            .Where(q => q.Id != Guid.Empty)
            .Select(q => q.Id)
            .ToList();

        // Remove questions that are no longer in the request
        var questionsToRemove = step.QuizQuestions
            .Where(q => !requestQuestionIds.Contains(q.Id))
            .ToList();

        foreach (var question in questionsToRemove)
        {
            step.QuizQuestions.Remove(question);
        }

        // Update or add quiz questions
        foreach (var questionDto in quizQuestions)
        {
            var existingQuestion = step.QuizQuestions.FirstOrDefault(q => q.Id == questionDto.Id && questionDto.Id != Guid.Empty);

            if (existingQuestion != null)
            {
                // Update existing question
                existingQuestion.Question = questionDto.Question;
                existingQuestion.Options = questionDto.Options;
                existingQuestion.Order = questionDto.Order;
            }
            else
            {
                // Add new question
                var newQuestion = new QuizQuestion
                {
                    Id = Guid.NewGuid(),
                    Question = questionDto.Question,
                    Options = questionDto.Options,
                    Order = questionDto.Order
                };
                step.QuizQuestions.Add(newQuestion);
            }
        }
    }
}

