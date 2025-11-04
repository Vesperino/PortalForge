using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Users.Commands.TransferDepartment;

/// <summary>
/// Command to transfer an employee to a different department.
/// Automatically reassigns pending requests to the new supervisor.
/// </summary>
public class TransferEmployeeToDepartmentCommand : IRequest<Unit>, ITransactionalRequest
{
    /// <summary>
    /// ID of the user being transferred.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// ID of the new department.
    /// </summary>
    public Guid NewDepartmentId { get; set; }

    /// <summary>
    /// ID of the new supervisor (optional).
    /// </summary>
    public Guid? NewSupervisorId { get; set; }

    /// <summary>
    /// ID of the user performing the transfer (usually HR or Admin).
    /// </summary>
    public Guid TransferredByUserId { get; set; }

    /// <summary>
    /// Reason for the transfer (optional).
    /// </summary>
    public string? Reason { get; set; }
}
