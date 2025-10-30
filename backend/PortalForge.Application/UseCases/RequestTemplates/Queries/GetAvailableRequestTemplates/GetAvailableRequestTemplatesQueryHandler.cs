using MediatR;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.RequestTemplates.DTOs;

namespace PortalForge.Application.UseCases.RequestTemplates.Queries.GetAvailableRequestTemplates;

public class GetAvailableRequestTemplatesQueryHandler 
    : IRequestHandler<GetAvailableRequestTemplatesQuery, GetAvailableRequestTemplatesResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAvailableRequestTemplatesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetAvailableRequestTemplatesResult> Handle(
        GetAvailableRequestTemplatesQuery request, 
        CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            return new GetAvailableRequestTemplatesResult();
        }

        var templates = await _unitOfWork.RequestTemplateRepository
            .GetAvailableForUserAsync(user.Department);

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

        return new GetAvailableRequestTemplatesResult
        {
            Templates = templateDtos
        };
    }
}

