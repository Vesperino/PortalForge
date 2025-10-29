using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.Admin.Queries.GetRoleGroups;

namespace PortalForge.Application.UseCases.Admin.Queries.GetPermissions;

public class GetPermissionsQueryHandler : IRequestHandler<GetPermissionsQuery, GetPermissionsResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetPermissionsQueryHandler> _logger;

    public GetPermissionsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetPermissionsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<GetPermissionsResult> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all permissions");

        var permissions = (await _unitOfWork.PermissionRepository.GetAllAsync()).ToList();

        // Filter by category if specified
        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            permissions = permissions.Where(p => p.Category == request.Category).ToList();
        }

        var permissionDtos = permissions.Select(p => new PermissionDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Category = p.Category
        }).ToList();

        // Group by category
        var permissionsByCategory = permissionDtos
            .GroupBy(p => p.Category)
            .ToDictionary(g => g.Key, g => g.ToList());

        _logger.LogInformation("Found {Count} permissions in {CategoryCount} categories",
            permissionDtos.Count, permissionsByCategory.Count);

        return new GetPermissionsResult
        {
            Permissions = permissionDtos,
            PermissionsByCategory = permissionsByCategory
        };
    }
}

