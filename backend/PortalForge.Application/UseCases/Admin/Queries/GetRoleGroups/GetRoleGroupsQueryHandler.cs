using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Admin.Queries.GetRoleGroups;

public class GetRoleGroupsQueryHandler : IRequestHandler<GetRoleGroupsQuery, GetRoleGroupsResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetRoleGroupsQueryHandler> _logger;

    public GetRoleGroupsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetRoleGroupsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<GetRoleGroupsResult> Handle(GetRoleGroupsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting role groups: SearchTerm={SearchTerm}, IsSystemRole={IsSystemRole}, Page={Page}",
            request.SearchTerm, request.IsSystemRole, request.PageNumber);

        var (roleGroups, totalCount) = await _unitOfWork.RoleGroupRepository.GetFilteredAsync(
            request.SearchTerm,
            request.IsSystemRole,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var roleGroupsList = roleGroups.ToList();
        var roleGroupIds = roleGroupsList.Select(rg => rg.Id).ToList();

        var allUserRoleGroups = await _unitOfWork.UserRoleGroupRepository.GetByRoleGroupIdsAsync(roleGroupIds);
        var userCountLookup = allUserRoleGroups
            .GroupBy(urg => urg.RoleGroupId)
            .ToDictionary(g => g.Key, g => g.Count());

        Dictionary<Guid, List<PermissionDto>> permissionsLookup = new();
        if (request.IncludePermissions)
        {
            var allRoleGroupPermissions = await _unitOfWork.RoleGroupPermissionRepository.GetByRoleGroupIdsAsync(roleGroupIds);

            permissionsLookup = allRoleGroupPermissions
                .GroupBy(rgp => rgp.RoleGroupId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(rgp => new PermissionDto
                    {
                        Id = rgp.Permission.Id,
                        Name = rgp.Permission.Name,
                        Description = rgp.Permission.Description,
                        Category = rgp.Permission.Category
                    }).ToList()
                );
        }

        var roleGroupDtos = roleGroupsList.Select(roleGroup => new RoleGroupDto
        {
            Id = roleGroup.Id,
            Name = roleGroup.Name,
            Description = roleGroup.Description,
            IsSystemRole = roleGroup.IsSystemRole,
            CreatedAt = roleGroup.CreatedAt,
            UpdatedAt = roleGroup.UpdatedAt,
            Permissions = permissionsLookup.TryGetValue(roleGroup.Id, out var permissions) ? permissions : new List<PermissionDto>(),
            UserCount = userCountLookup.TryGetValue(roleGroup.Id, out var count) ? count : 0
        }).ToList();

        _logger.LogInformation("Found {Count} role groups (total: {TotalCount})", roleGroupDtos.Count, totalCount);

        return new GetRoleGroupsResult
        {
            RoleGroups = roleGroupDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

