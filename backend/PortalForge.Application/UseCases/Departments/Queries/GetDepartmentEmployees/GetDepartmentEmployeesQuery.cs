using MediatR;
using PortalForge.Application.DTOs;

namespace PortalForge.Application.UseCases.Departments.Queries.GetDepartmentEmployees;

/// <summary>
/// Query to get all employees in a department.
/// </summary>
public class GetDepartmentEmployeesQuery : IRequest<List<EmployeeDto>>
{
    /// <summary>
    /// The ID of the department.
    /// </summary>
    public Guid DepartmentId { get; set; }

    /// <summary>
    /// Whether to include inactive employees.
    /// </summary>
    public bool IncludeInactive { get; set; } = false;
}
