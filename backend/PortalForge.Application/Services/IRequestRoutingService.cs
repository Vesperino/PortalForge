using PortalForge.Domain.Entities;

namespace PortalForge.Application.Services;

/// <summary>
/// Service responsible for resolving approvers in request workflows.
/// Implements intelligent routing based on organizational hierarchy.
/// </summary>
public interface IRequestRoutingService
{
    /// <summary>
    /// Resolves the approver for a specific approval step based on the template configuration.
    /// </summary>
    /// <param name="stepTemplate">The approval step template containing routing rules.</param>
    /// <param name="submitter">The user who submitted the request.</param>
    /// <returns>
    /// The user who should approve this step, or null if no approver can be found
    /// (which triggers auto-approval).
    /// </returns>
    Task<User?> ResolveApproverAsync(RequestApprovalStepTemplate stepTemplate, User submitter);

    /// <summary>
    /// Checks if a user has a higher supervisor in the hierarchy.
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <returns>True if the user has a supervisor, false otherwise.</returns>
    Task<bool> HasHigherSupervisorAsync(User user);

    /// <summary>
    /// Validates that a user has all required approvers in the organizational structure
    /// before allowing request submission.
    /// </summary>
    /// <param name="userId">The ID of the user submitting the request.</param>
    /// <param name="stepTemplates">The approval step templates that define required approvers.</param>
    /// <returns>
    /// A tuple containing:
    /// - IsValid: true if all required approvers exist, false otherwise
    /// - Errors: list of specific validation error messages
    /// </returns>
    Task<(bool IsValid, List<string> Errors)> ValidateApprovalStructureAsync(
        Guid userId,
        IEnumerable<RequestApprovalStepTemplate> stepTemplates);

    /// <summary>
    /// Resolves the approver for a step, with support for substitute approvers.
    /// If the primary approver is unavailable (e.g., on vacation), returns their substitute.
    /// </summary>
    /// <param name="stepTemplate">The approval step template.</param>
    /// <param name="submitter">The user submitting the request.</param>
    /// <param name="checkAvailability">Whether to check if approver is on vacation and use substitute.</param>
    /// <returns>The user ID of the approver (or substitute), or null if no approver can be found.</returns>
    Task<Guid?> GetApproverForStepWithSubstituteAsync(
        RequestApprovalStepTemplate stepTemplate,
        User submitter,
        bool checkAvailability = true);
}
