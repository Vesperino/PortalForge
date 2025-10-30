namespace PortalForge.Domain.Enums;

/// <summary>
/// Defines how approvers are assigned to an approval step.
/// </summary>
public enum ApproverType
{
    /// <summary>
    /// Approver is determined by hierarchical role (Manager, Director).
    /// Uses the submitter's organizational hierarchy.
    /// </summary>
    Role,
    
    /// <summary>
    /// Approver is a specific user selected by ID.
    /// Always routes to the same person regardless of submitter.
    /// </summary>
    SpecificUser,
    
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


