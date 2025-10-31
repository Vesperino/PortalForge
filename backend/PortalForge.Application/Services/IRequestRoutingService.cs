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
}
