namespace PortalForge.Domain.Entities;

/// <summary>
/// Represents a category for grouping internal services.
/// </summary>
public class InternalServiceCategory
{
    /// <summary>
    /// Unique identifier for the category.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the category (e.g., "IT Tools", "HR Systems", "Finance").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the category.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Icon for the category (emoji or icon name).
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Display order for sorting.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Timestamp when the category was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Collection of services in this category.
    /// </summary>
    public ICollection<InternalService> Services { get; set; } = new List<InternalService>();
}
