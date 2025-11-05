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
    /// For ApproverType.SpecificDepartment: The department whose head must approve.
    /// Null for other approver types.
    /// </summary>
    public Guid? SpecificDepartmentId { get; set; }
    public Department? SpecificDepartment { get; set; }

    /// <summary>
    /// Whether this step requires completing a quiz before approval.
    /// </summary>
    public bool RequiresQuiz { get; set; } = false;

    /// <summary>
    /// Whether this approval step can be processed in parallel with other steps in the same group.
    /// </summary>
    public bool IsParallel { get; set; } = false;

    /// <summary>
    /// Groups parallel approval steps together. Steps with the same ParallelGroupId can be processed simultaneously.
    /// Null for sequential steps.
    /// </summary>
    public string? ParallelGroupId { get; set; }

    /// <summary>
    /// For parallel approval groups, the minimum number of approvals required to proceed.
    /// Default is 1 (any one approver can approve for the group).
    /// </summary>
    public int MinimumApprovals { get; set; } = 1;

    /// <summary>
    /// Time limit for this approval step before escalation occurs.
    /// Null means no escalation timeout.
    /// </summary>
    public TimeSpan? EscalationTimeout { get; set; }

    /// <summary>
    /// User to escalate to if EscalationTimeout is exceeded.
    /// Null means no escalation user defined.
    /// </summary>
    public Guid? EscalationUserId { get; set; }
    public User? EscalationUser { get; set; }

    public DateTime CreatedAt { get; set; }
}

