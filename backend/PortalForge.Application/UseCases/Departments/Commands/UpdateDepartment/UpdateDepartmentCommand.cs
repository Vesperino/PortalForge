using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Departments.Commands.UpdateDepartment;

/// <summary>
/// Command to update an existing department.
/// </summary>
public class UpdateDepartmentCommand : IRequest<Unit>, ITransactionalRequest
{
    public Guid DepartmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public Guid? DepartmentHeadId { get; set; }
    public bool IsActive { get; set; }
}
