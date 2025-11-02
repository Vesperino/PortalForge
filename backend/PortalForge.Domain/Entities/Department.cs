using System;
using System.Collections.Generic;

namespace PortalForge.Domain.Entities;

/// <summary>
/// Represents a department in the organizational structure.
/// Supports unlimited hierarchical depth through self-referencing.
/// </summary>
public class Department
{
    /// <summary>
    /// Unique identifier for the department.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the department (e.g., "IT Department", "Development Team").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the department's responsibilities.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    // ===== HIERARCHY =====

    /// <summary>
    /// Parent department ID for hierarchical structure. Null for root departments.
    /// </summary>
    public Guid? ParentDepartmentId { get; set; }

    /// <summary>
    /// Navigation property to parent department.
    /// </summary>
    public Department? ParentDepartment { get; set; }

    /// <summary>
    /// Collection of child departments under this department.
    /// </summary>
    public ICollection<Department> ChildDepartments { get; set; } = new List<Department>();

    // ===== MANAGEMENT =====

    /// <summary>
    /// User ID of the department head/manager.
    /// </summary>
    public Guid? HeadOfDepartmentId { get; set; }

    /// <summary>
    /// Navigation property to the user who is head of this department.
    /// </summary>
    public User? HeadOfDepartment { get; set; }


    /// <summary>
    /// User ID of the department director (optional).
    /// </summary>
    public Guid? DirectorId { get; set; }

    /// <summary>
    /// Navigation property to the user who is director of this department.
    /// </summary>
    public User? Director { get; set; }

    // ===== STATUS =====

    /// <summary>
    /// Indicates whether the department is active. Used for soft delete.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Timestamp when the department was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the department was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    // ===== NAVIGATION =====

    /// <summary>
    /// Collection of employees assigned to this department.
    /// </summary>
    public ICollection<User> Employees { get; set; } = new List<User>();

    /// <summary>
    /// Many-to-many relationship with internal services.
    /// </summary>
    public ICollection<InternalServiceDepartment> DepartmentServices { get; set; } = new List<InternalServiceDepartment>();
}




