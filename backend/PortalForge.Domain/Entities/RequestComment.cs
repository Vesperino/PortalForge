namespace PortalForge.Domain.Entities;

/// <summary>
/// Represents a comment on a request.
/// Used for communication between submitter and approvers.
/// </summary>
public class RequestComment
{
    /// <summary>
    /// Unique identifier for the comment.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the request this comment belongs to.
    /// </summary>
    public Guid RequestId { get; set; }

    /// <summary>
    /// Navigation property to the request.
    /// </summary>
    public Request Request { get; set; } = null!;

    /// <summary>
    /// ID of the user who posted this comment.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property to the user who posted the comment.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// The comment text.
    /// </summary>
    public string Comment { get; set; } = string.Empty;

    /// <summary>
    /// JSON array of attachment file paths/URLs (optional).
    /// Used for screenshots, documents, etc. to support the comment.
    /// </summary>
    public string? Attachments { get; set; }

    /// <summary>
    /// When this comment was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
