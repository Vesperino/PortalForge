namespace PortalForge.Application.DTOs;

/// <summary>
/// DTO for department information.
/// </summary>
public class DepartmentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public string? ParentDepartmentName { get; set; }
    public Guid? DepartmentHeadId { get; set; }
    public string? DepartmentHeadName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int Level { get; set; }
    public int EmployeeCount { get; set; }
}

/// <summary>
/// DTO for department tree structure with children.
/// </summary>
public class DepartmentTreeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public Guid? DepartmentHeadId { get; set; }
    public string? DepartmentHeadName { get; set; }
    public bool IsActive { get; set; }
    public int Level { get; set; }
    public int EmployeeCount { get; set; }
    public List<DepartmentTreeDto> Children { get; set; } = new();
    public List<EmployeeDto> Employees { get; set; } = new();
}

/// <summary>
/// DTO for employee information in tree view.
/// </summary>
public class EmployeeDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Position { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO for creating a new department.
/// </summary>
public class CreateDepartmentDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public Guid? DepartmentHeadId { get; set; }
}

/// <summary>
/// DTO for updating an existing department.
/// </summary>
public class UpdateDepartmentDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public Guid? DepartmentHeadId { get; set; }
    public bool IsActive { get; set; }
}
