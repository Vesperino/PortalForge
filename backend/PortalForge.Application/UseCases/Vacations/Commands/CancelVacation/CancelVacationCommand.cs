using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Vacations.Commands.CancelVacation;

/// <summary>
/// Command to cancel an active vacation schedule.
/// Admin can cancel anytime. Approvers can cancel up to 1 day after vacation starts.
/// </summary>
public class CancelVacationCommand : IRequest<Unit>, ITransactionalRequest
{
    public Guid VacationScheduleId { get; set; }
    public Guid CancelledByUserId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
