using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.RequestTemplates.DTOs;

namespace PortalForge.Application.UseCases.RequestTemplates.Queries.GetRequestTemplates;

public class GetRequestTemplatesQueryHandler
    : IRequestHandler<GetRequestTemplatesQuery, GetRequestTemplatesResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetRequestTemplatesQueryHandler> _logger;

    public GetRequestTemplatesQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetRequestTemplatesQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<GetRequestTemplatesResult> Handle(
        GetRequestTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting request templates: SearchTerm={SearchTerm}, Category={Category}, Page={Page}",
            request.SearchTerm, request.Category, request.PageNumber);

        var (templates, totalCount) = await _unitOfWork.RequestTemplateRepository.GetFilteredAsync(
            request.SearchTerm,
            request.Category,
            request.IsActive,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var templateDtos = templates.Select(t => new RequestTemplateDto
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            Icon = t.Icon,
            Category = t.Category,
            DepartmentId = t.DepartmentId,
            IsActive = t.IsActive,
            RequiresApproval = t.RequiresApproval,
            EstimatedProcessingDays = t.EstimatedProcessingDays,
            PassingScore = t.PassingScore,
            CreatedById = t.CreatedById,
            CreatedByName = t.CreatedBy?.FullName ?? string.Empty,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt,
            Fields = t.Fields.Select(f => new RequestTemplateFieldDto
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
            ApprovalStepTemplates = t.ApprovalStepTemplates.Select(ast => new RequestApprovalStepTemplateDto
            {
                Id = ast.Id,
                StepOrder = ast.StepOrder,
                ApproverType = ast.ApproverType.ToString(),
                SpecificUserId = ast.SpecificUserId,
                SpecificDepartmentId = ast.SpecificDepartmentId,
                SpecificDepartmentRoleType = ast.SpecificDepartmentRoleType.ToString(),
                ApproverGroupId = ast.ApproverGroupId,
                RequiresQuiz = ast.RequiresQuiz,
                PassingScore = ast.PassingScore,
                QuizQuestions = ast.QuizQuestions.Select(q => new QuizQuestionDto
                {
                    Id = q.Id,
                    Question = q.Question,
                    Options = q.Options,
                    Order = q.Order
                }).ToList()
            }).ToList()
        }).ToList();

        _logger.LogInformation("Found {Count} request templates (total: {TotalCount})", templateDtos.Count, totalCount);

        return new GetRequestTemplatesResult
        {
            Templates = templateDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

