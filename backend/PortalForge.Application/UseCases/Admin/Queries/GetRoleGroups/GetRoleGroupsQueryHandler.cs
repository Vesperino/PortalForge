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
        _logger.LogInformation("Getting all role groups");

        var roleGroups = (await _unitOfWork.RoleGroupRepository.GetAllAsync()).ToList();
        var roleGroupDtos = new List<RoleGroupDto>();

        foreach (var roleGroup in roleGroups)
        {
            var permissions = new List<PermissionDto>();

            if (request.IncludePermissions)
            {
                var roleGroupPermissions = await _unitOfWork.RoleGroupPermissionRepository.GetByRoleGroupIdAsync(roleGroup.Id);
                var permissionIds = roleGroupPermissions.Select(rgp => rgp.PermissionId).ToList();
                var allPermissions = (await _unitOfWork.PermissionRepository.GetAllAsync())
                    .Where(p => permissionIds.Contains(p.Id))
                    .ToList();

                permissions = allPermissions.Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Category = p.Category
                }).ToList();
            }

            // Count users in this role group
            var userRoleGroups = await _unitOfWork.UserRoleGroupRepository.GetByRoleGroupIdAsync(roleGroup.Id);
            var userCount = userRoleGroups.Count();

            roleGroupDtos.Add(new RoleGroupDto
            {
                Id = roleGroup.Id,
                Name = roleGroup.Name,
                Description = roleGroup.Description,
                IsSystemRole = roleGroup.IsSystemRole,
                CreatedAt = roleGroup.CreatedAt,
                UpdatedAt = roleGroup.UpdatedAt,
                Permissions = permissions,
                UserCount = userCount
            });
        }

        _logger.LogInformation("Found {Count} role groups", roleGroupDtos.Count);

        return new GetRoleGroupsResult
        {
            RoleGroups = roleGroupDtos
        };
    }
}

