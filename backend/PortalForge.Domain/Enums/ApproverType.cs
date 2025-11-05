namespace PortalForge.Domain.Enums;

/// <summary>
/// Defines how approvers are assigned to an approval step.
/// Simplified to use only organizational structure (supervisors and department heads).
/// </summary>
public enum ApproverType
{
    /// <summary>
    /// Approver is the department head (manager) from submitter's department.
    /// Routes to Department.HeadOfDepartmentId from the submitter's department.
    /// </summary>
    DirectSupervisor,

    /// <summary>
    /// Approver is the department director from submitter's department.
    /// Routes to Department.DirectorId from the submitter's department.
    /// </summary>
    DepartmentDirector,

    /// <summary>
    /// Approver is a specific user selected by ID.
    /// Always routes to the same person regardless of submitter.
    /// </summary>
    SpecificUser,

    /// <summary>
    /// Approver is the head of a specific department.
    /// Routes to the user assigned as HeadOfDepartment for the specified department.
    /// </summary>
    SpecificDepartment,

    /// <summary>
    /// Approver can be any user from a specified RoleGroup.
    /// First available user from the group can approve.
    /// </summary>
    UserGroup,

    /// <summary>
    /// Approver is the person who submitted the request.
    /// Used for self-approval or acknowledgment scenarios.
    /// </summary>
    Submitter
}


