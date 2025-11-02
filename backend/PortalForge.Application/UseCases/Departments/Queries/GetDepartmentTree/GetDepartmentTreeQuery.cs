using MediatR;
using PortalForge.Application.DTOs;

namespace PortalForge.Application.UseCases.Departments.Queries.GetDepartmentTree;

/// <summary>
/// Query to get the department tree structure.
/// </summary>
public class GetDepartmentTreeQuery : IRequest<List<DepartmentTreeDto>>
{
    /// <summary>
    /// Whether to include inactive departments in the tree.
    /// </summary>
    public bool IncludeInactive { get; set; } = false;

    /// <summary>
    /// Optional user ID for filtering departments based on organizational permissions.
    /// If not provided, all departments will be returned (for admins/anonymous).
    /// </summary>
    public Guid? UserId { get; set; }
}
