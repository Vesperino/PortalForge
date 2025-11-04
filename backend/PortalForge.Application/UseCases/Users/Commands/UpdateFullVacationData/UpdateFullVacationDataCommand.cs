using MediatR;

namespace PortalForge.Application.UseCases.Users.Commands.UpdateFullVacationData;

/// <summary>
/// Command to update ALL vacation-related fields for a user.
/// Useful for migrations or admin corrections.
/// </summary>
public class UpdateFullVacationDataCommand : IRequest<Unit>
{
    public Guid UserId { get; set; }

    /// <summary>
    /// Annual vacation days entitlement (e.g., 26)
    /// </summary>
    public int AnnualVacationDays { get; set; }

    /// <summary>
    /// Number of vacation days already used this year
    /// </summary>
    public int VacationDaysUsed { get; set; }

    /// <summary>
    /// Number of on-demand vacation days already used (max 4)
    /// </summary>
    public int OnDemandVacationDaysUsed { get; set; }

    /// <summary>
    /// Number of circumstantial leave days used
    /// </summary>
    public int CircumstantialLeaveDaysUsed { get; set; }

    /// <summary>
    /// Vacation days carried over from previous year
    /// </summary>
    public int CarriedOverVacationDays { get; set; }

    /// <summary>
    /// Expiry date for carried over vacation days (typically September 30)
    /// </summary>
    public DateTime? CarriedOverExpiryDate { get; set; }

    /// <summary>
    /// User ID of the admin/HR making the change
    /// </summary>
    public Guid RequestedByUserId { get; set; }

    /// <summary>
    /// Reason for the update (e.g., "Migration from old system", "Manual correction")
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// IP address of the requester (for audit log)
    /// </summary>
    public string? IpAddress { get; set; }
}
