namespace PortalForge.Domain.Enums;

/// <summary>
/// Defines which role in a department should be selected as approver.
/// Used with ApproverType.SpecificDepartment to specify whether
/// the department head or director should approve.
/// </summary>
public enum DepartmentRoleType
{
    /// <summary>
    /// Department head (manager) - mapped to Department.HeadOfDepartmentId.
    /// </summary>
    Head,

    /// <summary>
    /// Department director - mapped to Department.DirectorId.
    /// </summary>
    Director
}
