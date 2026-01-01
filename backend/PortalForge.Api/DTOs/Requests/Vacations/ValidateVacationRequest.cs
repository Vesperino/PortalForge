using PortalForge.Domain.Enums;

namespace PortalForge.Api.DTOs.Requests.Vacations;

public sealed class ValidateVacationRequest
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public LeaveType LeaveType { get; init; }
}
