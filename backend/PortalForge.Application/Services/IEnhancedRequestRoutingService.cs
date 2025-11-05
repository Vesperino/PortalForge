using PortalForge.Domain.Entities;

namespace PortalForge.Application.Services;

/// <summary>
/// Enhanced service responsible for resolving approvers in request workflows.
/// Extends IRequestRoutingService with parallel approval, escalation, and delegation capabilities.
/// </summary>
public interface IEnhancedRequestRoutingService : IRequestRoutingService
{
    /// <summary>
    /// Resolves multiple approvers for parallel approval steps.
    /// </summary>
    /// <param name="stepTemplate">The approval step template containing parallel routing rules.</param>
    /// <param name="submitter">The user who submitted the request.</param>
    /// <returns>List of users who can approve this parallel step.</returns>
    Task<List<User>> ResolveParallelApproversAsync(RequestApprovalStepTemplate stepTemplate, User submitter);

    /// <summary>
    /// Checks if an approval step should be escalated based on timeout rules.
    /// </summary>
    /// <param name="step">The approval step to check for escalation.</param>
    /// <returns>True if the step should be escalated, false otherwise.</returns>
    Task<bool> ShouldEscalateAsync(RequestApprovalStep step);

    /// <summary>
    /// Escalates an approval step to the designated escalation user.
    /// </summary>
    /// <param name="stepId">The ID of the approval step to escalate.</param>
    /// <returns>The escalated approval step.</returns>
    Task<RequestApprovalStep> EscalateRequestStepAsync(Guid stepId);

    /// <summary>
    /// Gets all users who can approve on behalf of the original approver through delegation.
    /// </summary>
    /// <param name="originalApproverId">The ID of the original approver.</param>
    /// <returns>List of users who have been delegated approval authority.</returns>
    Task<List<User>> GetDelegatedApproversAsync(Guid originalApproverId);

    /// <summary>
    /// Sets up approval delegation from one user to another.
    /// </summary>
    /// <param name="fromUserId">The user delegating their approval authority.</param>
    /// <param name="toUserId">The user receiving the delegated authority.</param>
    /// <param name="until">When the delegation expires (null for indefinite).</param>
    /// <param name="reason">Reason for the delegation.</param>
    /// <returns>The created approval delegation.</returns>
    Task<ApprovalDelegation> SetApprovalDelegationAsync(Guid fromUserId, Guid toUserId, DateTime? until, string? reason = null);

    /// <summary>
    /// Removes an active approval delegation.
    /// </summary>
    /// <param name="delegationId">The ID of the delegation to remove.</param>
    /// <returns>True if the delegation was successfully removed.</returns>
    Task<bool> RemoveApprovalDelegationAsync(Guid delegationId);

    /// <summary>
    /// Gets all active delegations for a user (both as delegator and delegate).
    /// </summary>
    /// <param name="userId">The user ID to check delegations for.</param>
    /// <returns>Tuple containing delegations from the user and delegations to the user.</returns>
    Task<(List<ApprovalDelegation> DelegationsFrom, List<ApprovalDelegation> DelegationsTo)> GetUserDelegationsAsync(Guid userId);

    /// <summary>
    /// Validates that parallel approval requirements are met for a group of steps.
    /// </summary>
    /// <param name="parallelGroupId">The parallel group ID to validate.</param>
    /// <param name="requestId">The request ID containing the parallel steps.</param>
    /// <returns>True if minimum approval requirements are met.</returns>
    Task<bool> ValidateParallelApprovalRequirementsAsync(string parallelGroupId, Guid requestId);

    /// <summary>
    /// Gets the effective approver for a step, considering delegation and availability.
    /// </summary>
    /// <param name="stepTemplate">The approval step template.</param>
    /// <param name="submitter">The user submitting the request.</param>
    /// <param name="considerDelegation">Whether to consider approval delegations.</param>
    /// <returns>The effective approver (may be different from the original due to delegation).</returns>
    Task<User?> GetEffectiveApproverAsync(RequestApprovalStepTemplate stepTemplate, User submitter, bool considerDelegation = true);
}