using MediatR;
using PortalForge.Application.DTOs;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Permissions.Queries.GetOrganizationalPermission;

/// <summary>
/// Handler for getting organizational permission
/// </summary>
public class GetOrganizationalPermissionQueryHandler
    : IRequestHandler<GetOrganizationalPermissionQuery, OrganizationalPermissionDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetOrganizationalPermissionQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the get organizational permission query
    /// </summary>
    public async Task<OrganizationalPermissionDto> Handle(
        GetOrganizationalPermissionQuery request,
        CancellationToken cancellationToken)
    {
        var permission = await _unitOfWork.OrganizationalPermissionRepository
            .GetByUserIdAsync(request.UserId);

        if (permission == null)
        {
            // Return default permission if not found
            // Default: user can only see their own department
            var user = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId);

            return new OrganizationalPermissionDto
            {
                UserId = request.UserId,
                CanViewAllDepartments = false,
                VisibleDepartmentIds = user?.DepartmentId != null
                    ? new List<Guid> { user.DepartmentId.Value }
                    : new List<Guid>()
            };
        }

        return new OrganizationalPermissionDto
        {
            UserId = permission.UserId,
            CanViewAllDepartments = permission.CanViewAllDepartments,
            VisibleDepartmentIds = permission.GetVisibleDepartmentIds()
        };
    }
}
