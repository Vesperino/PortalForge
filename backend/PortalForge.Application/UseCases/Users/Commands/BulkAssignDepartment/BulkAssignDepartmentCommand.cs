using MediatR;

namespace PortalForge.Application.UseCases.Users.Commands.BulkAssignDepartment;

/// <summary>
/// Command to bulk assign multiple employees to a department.
/// </summary>
public class BulkAssignDepartmentCommand : IRequest<int>
{
    public List<Guid> EmployeeIds { get; set; } = new();
    public Guid DepartmentId { get; set; }
}
