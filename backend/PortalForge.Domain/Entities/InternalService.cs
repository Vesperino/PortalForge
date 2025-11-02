namespace PortalForge.Domain.Entities;

/// <summary>
/// Represents an internal service/tool link that can be assigned to departments.
/// </summary>
public class InternalService
{
    /// <summary>
    /// Unique identifier for the service.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the service (e.g., "HR Management System", "Email Portal").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the service.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// URL to the service.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Icon for the service. Can be:
    /// - Emoji (Unicode character)
    /// - URL to uploaded image
    /// - Icon name from icon library
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Type of icon: 'emoji', 'image', or 'font'.
    /// </summary>
    public string IconType { get; set; } = "emoji";

    /// <summary>
    /// Category ID for grouping services.
    /// </summary>
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// Navigation property to category.
    /// </summary>
    public InternalServiceCategory? Category { get; set; }

    /// <summary>
    /// Display order for sorting within category.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Indicates whether the service is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Indicates if service is global (visible to all departments).
    /// If true, department assignments are ignored.
    /// </summary>
    public bool IsGlobal { get; set; } = false;

    /// <summary>
    /// Indicates if service should be pinned/highlighted.
    /// </summary>
    public bool IsPinned { get; set; } = false;

    /// <summary>
    /// User ID of the creator.
    /// </summary>
    public Guid CreatedById { get; set; }

    /// <summary>
    /// Navigation property to creator.
    /// </summary>
    public User CreatedBy { get; set; } = null!;

    /// <summary>
    /// Timestamp when the service was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the service was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Many-to-many relationship with departments.
    /// Empty if IsGlobal = true.
    /// </summary>
    public ICollection<InternalServiceDepartment> ServiceDepartments { get; set; } = new List<InternalServiceDepartment>();
}
