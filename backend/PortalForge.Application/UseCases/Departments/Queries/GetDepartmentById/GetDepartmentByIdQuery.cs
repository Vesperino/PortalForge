using MediatR;
using PortalForge.Application.DTOs;

namespace PortalForge.Application.UseCases.Departments.Queries.GetDepartmentById;

/// <summary>
/// Query to get a single department by ID.
/// </summary>
public class GetDepartmentByIdQuery : IRequest<DepartmentDto>
{
    /// <summary>
    /// The ID of the department to retrieve.
    /// </summary>
    public Guid DepartmentId { get; set; }
}
