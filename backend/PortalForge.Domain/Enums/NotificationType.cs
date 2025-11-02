namespace PortalForge.Domain.Enums;

/// <summary>
/// Types of notifications that can be sent to users.
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// A request is waiting for the user's approval.
    /// </summary>
    RequestPendingApproval,
    
    /// <summary>
    /// The user's request has been approved at a step.
    /// </summary>
    RequestApproved,
    
    /// <summary>
    /// The user's request has been rejected.
    /// </summary>
    RequestRejected,
    
    /// <summary>
    /// The user's request has been completed (all approvals done).
    /// </summary>
    RequestCompleted,
    
    /// <summary>
    /// Someone added a comment to the user's request.
    /// </summary>
    RequestCommented,
    
    /// <summary>
    /// System-level notification (maintenance, updates, etc.).
    /// </summary>
    System,
    
    /// <summary>
    /// Company-wide announcement.
    /// </summary>
    Announcement,

    /// <summary>
    /// User has been assigned as a substitute for someone's vacation.
    /// </summary>
    VacationCoverageAssigned,

    /// <summary>
    /// A vacation that the user is covering has started.
    /// </summary>
    VacationCoverageStarted,

    /// <summary>
    /// User's vacation has ended, they're back from vacation.
    /// </summary>
    VacationEnded
}


