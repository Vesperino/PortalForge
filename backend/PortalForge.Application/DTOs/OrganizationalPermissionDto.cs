namespace PortalForge.Application.DTOs;

/// <summary>
/// Data transfer object for organizational permissions
/// </summary>
public class OrganizationalPermissionDto
{
    /// <summary>
    /// User ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Whether user can view all departments
    /// </summary>
    public bool CanViewAllDepartments { get; set; }

    /// <summary>
    /// List of specific department IDs the user can view (if not viewing all)
    /// </summary>
    public List<Guid> VisibleDepartmentIds { get; set; } = new();
}
