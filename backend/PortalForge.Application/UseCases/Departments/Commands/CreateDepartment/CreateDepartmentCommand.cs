using MediatR;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.DTOs;

namespace PortalForge.Application.UseCases.Departments.Commands.CreateDepartment;

/// <summary>
/// Command to create a new department.
/// </summary>
public class CreateDepartmentCommand : IRequest<Guid>, ITransactionalRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public Guid? DepartmentHeadId { get; set; }
}
