namespace PortalForge.Application.Common.Settings;

/// <summary>
/// Configuration settings for vacation-related functionality.
/// Values are based on Polish Labor Law (Kodeks Pracy).
/// </summary>
public class VacationSettings
{
    /// <summary>
    /// Configuration section name for binding from appsettings.
    /// </summary>
    public const string SectionName = "Vacation";

    /// <summary>
    /// Default annual vacation days entitlement.
    /// According to Polish Labor Law: 26 days for employees with 10+ years of service.
    /// </summary>
    public int DefaultAnnualDays { get; set; } = 26;

    /// <summary>
    /// Maximum on-demand vacation days per year.
    /// According to Polish Labor Law: 4 days per year.
    /// </summary>
    public int MaxOnDemandDays { get; set; } = 4;

    /// <summary>
    /// Maximum circumstantial leave days per event.
    /// According to Polish Labor Law: typically 2 days.
    /// </summary>
    public int MaxCircumstantialDaysPerEvent { get; set; } = 2;
}
