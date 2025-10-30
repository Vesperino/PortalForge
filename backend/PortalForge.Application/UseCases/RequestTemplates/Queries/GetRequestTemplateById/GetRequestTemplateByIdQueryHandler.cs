using MediatR;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.RequestTemplates.DTOs;

namespace PortalForge.Application.UseCases.RequestTemplates.Queries.GetRequestTemplateById;

public class GetRequestTemplateByIdQueryHandler 
    : IRequestHandler<GetRequestTemplateByIdQuery, GetRequestTemplateByIdResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetRequestTemplateByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetRequestTemplateByIdResult> Handle(
        GetRequestTemplateByIdQuery request, 
        CancellationToken cancellationToken)
    {
        var template = await _unitOfWork.RequestTemplateRepository.GetByIdAsync(request.Id);
        
        if (template == null)
        {
            return new GetRequestTemplateByIdResult();
        }

        var templateDto = new RequestTemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            Icon = template.Icon,
            Category = template.Category,
            DepartmentId = template.DepartmentId,
            IsActive = template.IsActive,
            RequiresApproval = template.RequiresApproval,
            EstimatedProcessingDays = template.EstimatedProcessingDays,
            PassingScore = template.PassingScore,
            CreatedById = template.CreatedById,
            CreatedByName = template.CreatedBy?.FullName ?? string.Empty,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt,
            Fields = template.Fields.Select(f => new RequestTemplateFieldDto
            {
                Id = f.Id,
                Label = f.Label,
                FieldType = f.FieldType.ToString(),
                Placeholder = f.Placeholder,
                IsRequired = f.IsRequired,
                Options = f.Options,
                MinValue = f.MinValue,
                MaxValue = f.MaxValue,
                HelpText = f.HelpText,
                Order = f.Order
            }).ToList(),
            ApprovalStepTemplates = template.ApprovalStepTemplates.Select(ast => new RequestApprovalStepTemplateDto
            {
                Id = ast.Id,
                StepOrder = ast.StepOrder,
                ApproverType = ast.ApproverType.ToString(),
                ApproverRole = ast.ApproverRole?.ToString(),
                SpecificUserId = ast.SpecificUserId,
                ApproverGroupId = ast.ApproverGroupId,
                RequiresQuiz = ast.RequiresQuiz
            }).ToList(),
            QuizQuestions = template.QuizQuestions.Select(qq => new QuizQuestionDto
            {
                Id = qq.Id,
                Question = qq.Question,
                Options = qq.Options,
                Order = qq.Order
            }).ToList()
        };

        return new GetRequestTemplateByIdResult
        {
            Template = templateDto
        };
    }
}

