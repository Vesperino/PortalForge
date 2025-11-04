using System;
using PortalForge.Domain.Enums;

namespace PortalForge.Domain.Entities;

/// <summary>
/// Represents a scheduled vacation with automatic substitute assignment.
/// Created automatically when a vacation request is approved.
/// </summary>
public class VacationSchedule
{
    /// <summary>
    /// Unique identifier for the vacation schedule.
    /// </summary>
    public Guid Id { get; set; }

    // ===== WHO =====

    /// <summary>
    /// User who is on vacation.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property to the user on vacation.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// User who substitutes during vacation (handles approvals, etc.). Optional.
    /// </summary>
    public Guid? SubstituteUserId { get; set; }

    /// <summary>
    /// Navigation property to the substitute user.
    /// </summary>
    public User? Substitute { get; set; }

    // ===== WHEN =====

    /// <summary>
    /// Vacation start date (inclusive).
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Vacation end date (inclusive).
    /// </summary>
    public DateTime EndDate { get; set; }

    // ===== SOURCE =====

    /// <summary>
    /// Link to the approved vacation request that created this schedule.
    /// </summary>
    public Guid SourceRequestId { get; set; }

    /// <summary>
    /// Navigation property to the source request.
    /// </summary>
    public Request SourceRequest { get; set; } = null!;

    // ===== STATUS =====

    /// <summary>
    /// Current status of the vacation.
    /// Scheduled → Active → Completed (via daily job).
    /// </summary>
    public VacationStatus Status { get; set; } = VacationStatus.Scheduled;

    /// <summary>
    /// Timestamp when the vacation schedule was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    // ===== COMPUTED =====

    /// <summary>
    /// Number of vacation days (including start and end date).
    /// </summary>
    public int DaysCount => (EndDate.Date - StartDate.Date).Days + 1;

    /// <summary>
    /// Whether vacation is currently active (today is within start and end date).
    /// </summary>
    public bool IsActive => Status == VacationStatus.Active;
}
