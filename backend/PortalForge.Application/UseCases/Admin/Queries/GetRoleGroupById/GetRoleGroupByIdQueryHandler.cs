using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Exceptions;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.Admin.Queries.GetRoleGroups;

namespace PortalForge.Application.UseCases.Admin.Queries.GetRoleGroupById;

public class GetRoleGroupByIdQueryHandler : IRequestHandler<GetRoleGroupByIdQuery, GetRoleGroupByIdResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetRoleGroupByIdQueryHandler> _logger;

    public GetRoleGroupByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetRoleGroupByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<GetRoleGroupByIdResult> Handle(GetRoleGroupByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting role group by ID: {RoleGroupId}", request.RoleGroupId);

        var roleGroup = await _unitOfWork.RoleGroupRepository.GetByIdAsync(request.RoleGroupId);
        if (roleGroup == null)
        {
            throw new NotFoundException($"Role group with ID {request.RoleGroupId} not found");
        }

        // Map permissions
        var permissions = roleGroup.RoleGroupPermissions
            .Select(rgp => new PermissionDto
            {
                Id = rgp.Permission.Id,
                Name = rgp.Permission.Name,
                Description = rgp.Permission.Description,
                Category = rgp.Permission.Category
            })
            .ToList();

        // Count users in this role group
        var userRoleGroups = await _unitOfWork.UserRoleGroupRepository.GetByRoleGroupIdAsync(roleGroup.Id);
        var userCount = userRoleGroups.Count();

        var roleGroupDto = new RoleGroupDto
        {
            Id = roleGroup.Id,
            Name = roleGroup.Name,
            Description = roleGroup.Description,
            IsSystemRole = roleGroup.IsSystemRole,
            CreatedAt = roleGroup.CreatedAt,
            UpdatedAt = roleGroup.UpdatedAt,
            Permissions = permissions,
            UserCount = userCount
        };

        _logger.LogInformation("Found role group: {RoleGroupId}", roleGroup.Id);

        return new GetRoleGroupByIdResult
        {
            RoleGroup = roleGroupDto
        };
    }
}

