using MediatR;
using PortalForge.Application.DTOs;

namespace PortalForge.Application.UseCases.Permissions.Queries.GetOrganizationalPermission;

/// <summary>
/// Query to get organizational permission for a user
/// </summary>
public class GetOrganizationalPermissionQuery : IRequest<OrganizationalPermissionDto>
{
    /// <summary>
    /// User ID to get permissions for
    /// </summary>
    public Guid UserId { get; set; }
}
