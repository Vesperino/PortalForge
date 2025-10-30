using PortalForge.Domain.Enums;

namespace PortalForge.Domain.Entities;

/// <summary>
/// Template for an approval step in a request workflow.
/// Defines who should approve and under what conditions.
/// </summary>
public class RequestApprovalStepTemplate
{
    public Guid Id { get; set; }
    public Guid RequestTemplateId { get; set; }
    public RequestTemplate RequestTemplate { get; set; } = null!;

    /// <summary>
    /// Order of this step in the approval workflow (1, 2, 3, ...).
    /// </summary>
    public int StepOrder { get; set; }

    /// <summary>
    /// How the approver(s) should be determined.
    /// </summary>
    public ApproverType ApproverType { get; set; } = ApproverType.Role;

    /// <summary>
    /// For ApproverType.Role: The hierarchical role (Manager, Director).
    /// Null for other approver types.
    /// </summary>
    public DepartmentRole? ApproverRole { get; set; }

    /// <summary>
    /// For ApproverType.SpecificUser: The specific user who must approve.
    /// Null for other approver types.
    /// </summary>
    public Guid? SpecificUserId { get; set; }
    public User? SpecificUser { get; set; }

    /// <summary>
    /// For ApproverType.UserGroup: The group from which any member can approve.
    /// Null for other approver types.
    /// </summary>
    public Guid? ApproverGroupId { get; set; }
    public RoleGroup? ApproverGroup { get; set; }

    /// <summary>
    /// Whether this step requires completing a quiz before approval.
    /// </summary>
    public bool RequiresQuiz { get; set; } = false;

    public DateTime CreatedAt { get; set; }
}

