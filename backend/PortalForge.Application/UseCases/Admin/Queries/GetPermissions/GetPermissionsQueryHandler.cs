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
        _logger.LogInformation("Getting permissions: SearchTerm={SearchTerm}, Category={Category}, Page={Page}",
            request.SearchTerm, request.Category, request.PageNumber);

        var (permissions, totalCount) = await _unitOfWork.PermissionRepository.GetFilteredAsync(
            request.SearchTerm,
            request.Category,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var permissionDtos = permissions.Select(p => new PermissionDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Category = p.Category
        }).ToList();

        var permissionsByCategory = permissionDtos
            .GroupBy(p => p.Category)
            .ToDictionary(g => g.Key, g => g.ToList());

        _logger.LogInformation("Found {Count} permissions (total: {TotalCount})",
            permissionDtos.Count, totalCount);

        return new GetPermissionsResult
        {
            Permissions = permissionDtos,
            PermissionsByCategory = permissionsByCategory,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

