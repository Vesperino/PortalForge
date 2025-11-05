namespace PortalForge.Application.DTOs;

/// <summary>
/// Result of circumstantial leave validation according to Polish labor law.
/// </summary>
public class CircumstantialLeaveResult
{
    /// <summary>
    /// Whether the circumstantial leave request is valid.
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
    /// Whether documentation is required for this type of leave.
    /// </summary>
    public bool DocumentationRequired { get; set; }

    /// <summary>
    /// Whether the provided documentation is sufficient.
    /// </summary>
    public bool DocumentationSufficient { get; set; }

    /// <summary>
    /// The validated reason category for the leave.
    /// </summary>
    public string? ReasonCategory { get; set; }

    /// <summary>
    /// Maximum allowed days for this type of circumstantial leave.
    /// </summary>
    public int MaxAllowedDays { get; set; }

    /// <summary>
    /// Additional information or warnings about the leave request.
    /// </summary>
    public string? AdditionalInfo { get; set; }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static CircumstantialLeaveResult Success(
        int daysRequested,
        string reasonCategory,
        int maxAllowedDays,
        bool documentationRequired = false,
        string? additionalInfo = null)
    {
        return new CircumstantialLeaveResult
        {
            IsValid = true,
            DaysRequested = daysRequested,
            ReasonCategory = reasonCategory,
            MaxAllowedDays = maxAllowedDays,
            DocumentationRequired = documentationRequired,
            DocumentationSufficient = !documentationRequired || true, // Assume sufficient if not required
            AdditionalInfo = additionalInfo
        };
    }

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    public static CircumstantialLeaveResult Failure(string errorMessage, int daysRequested = 0)
    {
        return new CircumstantialLeaveResult
        {
            IsValid = false,
            ErrorMessage = errorMessage,
            DaysRequested = daysRequested
        };
    }
}