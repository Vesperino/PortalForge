namespace PortalForge.Application.DTOs;

/// <summary>
/// Result of on-demand vacation validation according to Polish labor law.
/// Polish law allows maximum 4 days of on-demand vacation per year.
/// </summary>
public class OnDemandVacationResult
{
    /// <summary>
    /// Whether the on-demand vacation request is valid.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Error message if validation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Number of days requested.
    /// </summary>
    public int DaysRequested { get; set; }

    /// <summary>
    /// Number of on-demand vacation days already used this year.
    /// </summary>
    public int DaysUsedThisYear { get; set; }

    /// <summary>
    /// Number of on-demand vacation days remaining this year.
    /// </summary>
    public int DaysRemaining { get; set; }

    /// <summary>
    /// Maximum allowed on-demand vacation days per year (4 according to Polish law).
    /// </summary>
    public int MaxAllowedDaysPerYear { get; set; } = 4;

    /// <summary>
    /// The year this validation applies to.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Additional information about the request.
    /// </summary>
    public string? AdditionalInfo { get; set; }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static OnDemandVacationResult Success(
        int daysRequested,
        int daysUsedThisYear,
        int year,
        string? additionalInfo = null)
    {
        var daysRemaining = 4 - daysUsedThisYear - daysRequested;
        
        return new OnDemandVacationResult
        {
            IsValid = true,
            DaysRequested = daysRequested,
            DaysUsedThisYear = daysUsedThisYear,
            DaysRemaining = daysRemaining,
            Year = year,
            AdditionalInfo = additionalInfo
        };
    }

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    public static OnDemandVacationResult Failure(
        string errorMessage,
        int daysRequested,
        int daysUsedThisYear,
        int year)
    {
        var daysRemaining = Math.Max(0, 4 - daysUsedThisYear);
        
        return new OnDemandVacationResult
        {
            IsValid = false,
            ErrorMessage = errorMessage,
            DaysRequested = daysRequested,
            DaysUsedThisYear = daysUsedThisYear,
            DaysRemaining = daysRemaining,
            Year = year
        };
    }
}