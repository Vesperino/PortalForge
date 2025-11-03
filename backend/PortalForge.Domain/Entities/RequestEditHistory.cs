namespace PortalForge.Domain.Entities;

/// <summary>
/// Tracks the edit history of a request.
/// When a request is edited, a record is created showing what changed.
/// </summary>
public class RequestEditHistory
{
    /// <summary>
    /// Unique identifier for this history record.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the request that was edited.
    /// </summary>
    public Guid RequestId { get; set; }

    /// <summary>
    /// Navigation property to the request.
    /// </summary>
    public Request Request { get; set; } = null!;

    /// <summary>
    /// ID of the user who edited the request.
    /// </summary>
    public Guid EditedByUserId { get; set; }

    /// <summary>
    /// Navigation property to the user who edited the request.
    /// </summary>
    public User EditedBy { get; set; } = null!;

    /// <summary>
    /// When this edit was made.
    /// </summary>
    public DateTime EditedAt { get; set; }

    /// <summary>
    /// The form data before the edit (JSON string).
    /// </summary>
    public string OldFormData { get; set; } = string.Empty;

    /// <summary>
    /// The form data after the edit (JSON string).
    /// </summary>
    public string NewFormData { get; set; } = string.Empty;

    /// <summary>
    /// Optional reason for the change provided by the editor.
    /// </summary>
    public string? ChangeReason { get; set; }
}
