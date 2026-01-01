namespace PortalForge.Api.DTOs.Requests.Users;

public sealed class UpdateFullVacationDataRequest
{
    public int AnnualVacationDays { get; init; } = 26;
    public int VacationDaysUsed { get; init; }
    public int OnDemandVacationDaysUsed { get; init; }
    public int CircumstantialLeaveDaysUsed { get; init; }
    public int CarriedOverVacationDays { get; init; }
    public DateTime? CarriedOverExpiryDate { get; init; }
    public string Reason { get; init; } = string.Empty;
}
