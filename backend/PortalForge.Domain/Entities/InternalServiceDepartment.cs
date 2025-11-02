namespace PortalForge.Domain.Entities;

/// <summary>
/// Junction table for many-to-many relationship between InternalService and Department.
/// Allows services to be assigned to multiple departments.
/// </summary>
public class InternalServiceDepartment
{
    /// <summary>
    /// Internal service ID.
    /// </summary>
    public Guid InternalServiceId { get; set; }

    /// <summary>
    /// Navigation property to internal service.
    /// </summary>
    public InternalService InternalService { get; set; } = null!;

    /// <summary>
    /// Department ID.
    /// </summary>
    public Guid DepartmentId { get; set; }

    /// <summary>
    /// Navigation property to department.
    /// </summary>
    public Department Department { get; set; } = null!;

    /// <summary>
    /// Timestamp when the service was assigned to department.
    /// </summary>
    public DateTime AssignedAt { get; set; }
}
