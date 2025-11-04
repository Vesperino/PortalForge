using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Users.Commands.UpdateVacationAllowance;

/// <summary>
/// Command to update user's annual vacation allowance.
/// Only Admin/HR can execute this command.
/// Requires audit logging and user notification.
/// </summary>
public class UpdateUserVacationAllowanceCommand : IRequest<Unit>, ITransactionalRequest
{
    public Guid UserId { get; set; }
    public int NewAnnualDays { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid RequestedByUserId { get; set; }
    public string? IpAddress { get; set; }
}
