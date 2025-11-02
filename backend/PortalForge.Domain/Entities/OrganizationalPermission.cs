using System;
using System.Collections.Generic;
using System.Text.Json;

namespace PortalForge.Domain.Entities;

/// <summary>
/// Defines which departments a user can view.
/// By default, users can only see their own department.
/// Admins can grant additional visibility.
/// </summary>
public class OrganizationalPermission
{
    /// <summary>
    /// Unique identifier for the permission.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User this permission applies to.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property to the user.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// If true, user can view all departments in the organization.
    /// </summary>
    public bool CanViewAllDepartments { get; set; } = false;

    /// <summary>
    /// JSON array of Guid[] - specific department IDs the user can view.
    /// Stored as JSONB in PostgreSQL for efficient querying.
    /// Example: ["550e8400-e29b-41d4-a716-446655440000", "..."]
    /// </summary>
    public string VisibleDepartmentIds { get; set; } = "[]";

    /// <summary>
    /// Timestamp when the permission was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the permission was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    // ===== HELPER METHODS =====

    /// <summary>
    /// Parses the JSON array into a list of Guids.
    /// </summary>
    /// <returns>List of department IDs the user can view.</returns>
    public List<Guid> GetVisibleDepartmentIds()
    {
        return JsonSerializer.Deserialize<List<Guid>>(VisibleDepartmentIds) ?? new List<Guid>();
    }

    /// <summary>
    /// Serializes the list of department IDs into a JSON array.
    /// </summary>
    /// <param name="departmentIds">List of department IDs to set.</param>
    public void SetVisibleDepartmentIds(List<Guid> departmentIds)
    {
        VisibleDepartmentIds = JsonSerializer.Serialize(departmentIds);
    }
}
