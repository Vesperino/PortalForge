namespace PortalForge.Domain.Entities;

/// <summary>
/// Represents a job position/title in the organization.
/// </summary>
public class Position
{
    /// <summary>
    /// Unique identifier for the position.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Position name/title (e.g., "Senior Developer", "Marketing Manager").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the position.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this position is active and can be assigned to users.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Timestamp when the position was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the position was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Navigation property - users who have this position.
    /// </summary>
    public ICollection<User> Users { get; set; } = new List<User>();
}
