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
    public ApproverType ApproverType { get; set; } = ApproverType.DirectSupervisor;

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
    /// Minimum score (percentage) required to pass the quiz for this approval step.
    /// Only relevant if RequiresQuiz is true. Value between 0-100.
    /// </summary>
    public int? PassingScore { get; set; }

    /// <summary>
    /// Quiz questions specific to this approval step.
    /// Only relevant if RequiresQuiz is true.
    /// </summary>
    public ICollection<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();

    public DateTime CreatedAt { get; set; }
}

