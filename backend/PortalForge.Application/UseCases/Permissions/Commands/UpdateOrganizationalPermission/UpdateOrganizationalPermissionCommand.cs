using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Permissions.Commands.UpdateOrganizationalPermission;

/// <summary>
/// Command to update organizational permission for a user
/// </summary>
public class UpdateOrganizationalPermissionCommand : IRequest<Unit>, ITransactionalRequest
{
    /// <summary>
    /// User ID to update permissions for
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Whether user can view all departments
    /// </summary>
    public bool CanViewAllDepartments { get; set; }

    /// <summary>
    /// List of specific department IDs the user can view
    /// </summary>
    public List<Guid> VisibleDepartmentIds { get; set; } = new();
}
