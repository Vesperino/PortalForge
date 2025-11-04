using MediatR;

namespace PortalForge.Application.UseCases.Users.Queries.GetUserVacationSummary;

/// <summary>
/// Query to retrieve vacation summary for a user.
/// Returns current vacation allowance, used days, and remaining days.
/// </summary>
public class GetUserVacationSummaryQuery : IRequest<VacationSummaryDto>
{
    public Guid UserId { get; set; }
}

/// <summary>
/// DTO containing user's vacation summary information.
/// </summary>
public class VacationSummaryDto
{
    public int AnnualVacationDays { get; set; }
    public int VacationDaysUsed { get; set; }
    public int VacationDaysRemaining { get; set; }
    public int OnDemandVacationDaysUsed { get; set; }
    public int OnDemandVacationDaysRemaining { get; set; }
    public int CircumstantialLeaveDaysUsed { get; set; }
    public int CarriedOverVacationDays { get; set; }
    public DateTime? CarriedOverExpiryDate { get; set; }
    public int TotalAvailableVacationDays { get; set; }
}
