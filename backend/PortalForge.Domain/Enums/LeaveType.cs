namespace PortalForge.Domain.Enums;

/// <summary>
/// Types of leave that can be requested.
/// Based on Polish labor law (Kodeks Pracy).
/// </summary>
public enum LeaveType
{
    /// <summary>
    /// Annual vacation leave (urlop wypoczynkowy) - 26 days per year.
    /// </summary>
    Annual = 0,

    /// <summary>
    /// On-demand vacation (urlop na żądanie) - up to 4 days per year.
    /// Can be taken without prior notice. Auto-approved if days available.
    /// </summary>
    OnDemand = 1,

    /// <summary>
    /// Circumstantial leave (urlop okolicznościowy) - for special occasions.
    /// E.g., wedding, funeral, birth of child - typically 2 days.
    /// </summary>
    Circumstantial = 2,

    /// <summary>
    /// Sick leave (zwolnienie lekarskie / L4).
    /// Auto-approved per Polish law, employer cannot reject.
    /// </summary>
    Sick = 3,

    /// <summary>
    /// Other types of leave not categorized above.
    /// </summary>
    Other = 4
}
