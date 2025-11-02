using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Departments.Commands.DeleteDepartment;

/// <summary>
/// Command to soft delete a department (sets IsActive = false).
/// </summary>
public class DeleteDepartmentCommand : IRequest<Unit>, ITransactionalRequest
{
    public Guid DepartmentId { get; set; }
}
