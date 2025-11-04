using PortalForge.Domain.Enums;

namespace PortalForge.Domain.Entities;

/// <summary>
/// Represents a sick leave (L4 - zwolnienie lekarskie) record.
/// Created automatically when a sick leave request is approved.
/// Per Polish law, sick leave is automatically accepted and cannot be rejected by employer.
/// </summary>
public class SickLeave
{
    /// <summary>
    /// Unique identifier for the sick leave record.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the user who is on sick leave.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property to the user on sick leave.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Start date of the sick leave.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date of the sick leave.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Total number of days of sick leave.
    /// </summary>
    public int DaysCount { get; set; }

    /// <summary>
    /// Whether this sick leave requires ZUS (Social Insurance) document submission.
    /// Per Polish law, sick leave longer than 33 days requires ZUS certification.
    /// </summary>
    public bool RequiresZusDocument { get; set; }

    /// <summary>
    /// URL to uploaded ZUS document (if required and submitted).
    /// </summary>
    public string? ZusDocumentUrl { get; set; }

    /// <summary>
    /// ID of the source request that created this sick leave record.
    /// </summary>
    public Guid SourceRequestId { get; set; }

    /// <summary>
    /// Navigation property to the source request.
    /// </summary>
    public Request SourceRequest { get; set; } = null!;

    /// <summary>
    /// Current status of the sick leave.
    /// </summary>
    public SickLeaveStatus Status { get; set; } = SickLeaveStatus.Active;

    /// <summary>
    /// When this sick leave record was created in the system.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Optional notes about the sick leave.
    /// </summary>
    public string? Notes { get; set; }
}
