namespace PortalForge.Api.DTOs.Requests.Users;

public class UpdateFullVacationDataRequest
{
    /// <summary>
    /// Annual vacation days entitlement (e.g., 26)
    /// </summary>
    public int AnnualVacationDays { get; set; } = 26;

    /// <summary>
    /// Number of vacation days already used this year
    /// </summary>
    public int VacationDaysUsed { get; set; } = 0;

    /// <summary>
    /// Number of on-demand vacation days already used (max 4)
    /// </summary>
    public int OnDemandVacationDaysUsed { get; set; } = 0;

    /// <summary>
    /// Number of circumstantial leave days used
    /// </summary>
    public int CircumstantialLeaveDaysUsed { get; set; } = 0;

    /// <summary>
    /// Vacation days carried over from previous year
    /// </summary>
    public int CarriedOverVacationDays { get; set; } = 0;

    /// <summary>
    /// Expiry date for carried over vacation days (typically September 30)
    /// </summary>
    public DateTime? CarriedOverExpiryDate { get; set; }

    /// <summary>
    /// Reason for the update (e.g., "Migration from old system", "Manual correction")
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}
