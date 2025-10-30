using MediatR;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.RequestTemplates.DTOs;

namespace PortalForge.Application.UseCases.RequestTemplates.Queries.GetRequestTemplates;

public class GetRequestTemplatesQueryHandler 
    : IRequestHandler<GetRequestTemplatesQuery, GetRequestTemplatesResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetRequestTemplatesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetRequestTemplatesResult> Handle(
        GetRequestTemplatesQuery request, 
        CancellationToken cancellationToken)
    {
        var templates = await _unitOfWork.RequestTemplateRepository.GetAllAsync();

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
            UpdatedAt = t.UpdatedAt
        }).ToList();

        return new GetRequestTemplatesResult
        {
            Templates = templateDtos
        };
    }
}

