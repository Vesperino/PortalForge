using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.InternalServices.DTOs;

namespace PortalForge.Application.UseCases.InternalServices.Queries.GetServicesForUser;

public class GetServicesForUserQueryHandler : IRequestHandler<GetServicesForUserQuery, List<InternalServiceDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetServicesForUserQueryHandler> _logger;

    public GetServicesForUserQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetServicesForUserQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<InternalServiceDto>> Handle(GetServicesForUserQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting services for user: {UserId}", request.UserId);

        var user = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", request.UserId);
            return new List<InternalServiceDto>();
        }

        IEnumerable<Domain.Entities.InternalService> services;

        // Admin sees ALL active services
        if (user.Role == Domain.Entities.UserRole.Admin)
        {
            services = await _unitOfWork.InternalServiceRepository.GetActiveAsync();
        }
        else if (user.DepartmentId.HasValue)
        {
            services = await _unitOfWork.InternalServiceRepository.GetByDepartmentIdAsync(user.DepartmentId.Value);
        }
        else
        {
            services = await _unitOfWork.InternalServiceRepository.GetGlobalServicesAsync();
        }

        var result = services.Select(s => new InternalServiceDto
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            Url = s.Url,
            Icon = s.Icon,
            IconType = s.IconType,
            CategoryId = s.CategoryId,
            CategoryName = s.Category?.Name,
            DisplayOrder = s.DisplayOrder,
            IsActive = s.IsActive,
            IsGlobal = s.IsGlobal,
            IsPinned = s.IsPinned,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt,
            DepartmentIds = s.ServiceDepartments.Select(sd => sd.DepartmentId).ToList()
        }).ToList();

        _logger.LogInformation("Found {Count} services for user {UserId}", result.Count, request.UserId);

        return result;
    }
}
