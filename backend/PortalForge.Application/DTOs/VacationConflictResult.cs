using PortalForge.Domain.Entities;

namespace PortalForge.Application.DTOs;

/// <summary>
/// Result of vacation conflict detection with existing schedules and team coverage analysis.
/// </summary>
public class VacationConflictResult
{
    /// <summary>
    /// Whether there are any conflicts with the requested vacation dates.
    /// </summary>
    public bool HasConflicts { get; set; }

    /// <summary>
    /// List of specific conflicts detected.
    /// </summary>
    public List<VacationConflict> Conflicts { get; set; } = new();

    /// <summary>
    /// Team coverage analysis for the requested dates.
    /// </summary>
    public TeamCoverageAnalysis CoverageAnalysis { get; set; } = new();

    /// <summary>
    /// Whether the vacation can still be approved despite conflicts.
    /// </summary>
    public bool CanBeApproved { get; set; }

    /// <summary>
    /// Recommendations or warnings about the vacation request.
    /// </summary>
    public List<string> Recommendations { get; set; } = new();

    /// <summary>
    /// Creates a result with no conflicts.
    /// </summary>
    public static VacationConflictResult NoConflicts(TeamCoverageAnalysis coverageAnalysis)
    {
        return new VacationConflictResult
        {
            HasConflicts = false,
            CanBeApproved = true,
            CoverageAnalysis = coverageAnalysis
        };
    }

    /// <summary>
    /// Creates a result with conflicts.
    /// </summary>
    public static VacationConflictResult WithConflicts(
        List<VacationConflict> conflicts,
        TeamCoverageAnalysis coverageAnalysis,
        bool canBeApproved = false)
    {
        return new VacationConflictResult
        {
            HasConflicts = true,
            Conflicts = conflicts,
            CoverageAnalysis = coverageAnalysis,
            CanBeApproved = canBeApproved
        };
    }
}

/// <summary>
/// Represents a specific vacation conflict.
/// </summary>
public class VacationConflict
{
    /// <summary>
    /// Type of conflict detected.
    /// </summary>
    public VacationConflictType Type { get; set; }

    /// <summary>
    /// Description of the conflict.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Date range where the conflict occurs.
    /// </summary>
    public DateTime ConflictStartDate { get; set; }

    /// <summary>
    /// End date of the conflict.
    /// </summary>
    public DateTime ConflictEndDate { get; set; }

    /// <summary>
    /// Users involved in the conflict (e.g., other employees on vacation).
    /// </summary>
    public List<User> InvolvedUsers { get; set; } = new();

    /// <summary>
    /// Severity level of the conflict.
    /// </summary>
    public ConflictSeverity Severity { get; set; }
}

/// <summary>
/// Team coverage analysis for vacation dates.
/// </summary>
public class TeamCoverageAnalysis
{
    /// <summary>
    /// Total team size in the department.
    /// </summary>
    public int TeamSize { get; set; }

    /// <summary>
    /// Number of team members who will be on vacation during the requested dates.
    /// </summary>
    public int MembersOnVacation { get; set; }

    /// <summary>
    /// Number of team members available during the requested dates.
    /// </summary>
    public int MembersAvailable { get; set; }

    /// <summary>
    /// Coverage percentage (available members / total team size * 100).
    /// </summary>
    public double CoveragePercentage { get; set; }

    /// <summary>
    /// Whether the coverage is considered adequate (>70%).
    /// </summary>
    public bool IsAdequateCoverage { get; set; }

    /// <summary>
    /// List of dates with critical coverage issues.
    /// </summary>
    public List<DateTime> CriticalCoverageDates { get; set; } = new();
}

/// <summary>
/// Types of vacation conflicts.
/// </summary>
public enum VacationConflictType
{
    /// <summary>
    /// User already has vacation scheduled for overlapping dates.
    /// </summary>
    OverlappingVacation,

    /// <summary>
    /// Too many team members on vacation (coverage below threshold).
    /// </summary>
    InsufficientCoverage,

    /// <summary>
    /// Key team member or supervisor already on vacation.
    /// </summary>
    KeyPersonnelUnavailable,

    /// <summary>
    /// Vacation conflicts with important business dates or deadlines.
    /// </summary>
    BusinessCriticalPeriod
}

/// <summary>
/// Severity levels for vacation conflicts.
/// </summary>
public enum ConflictSeverity
{
    /// <summary>
    /// Low severity - vacation can be approved with minor considerations.
    /// </summary>
    Low,

    /// <summary>
    /// Medium severity - vacation requires additional approval or planning.
    /// </summary>
    Medium,

    /// <summary>
    /// High severity - vacation should be reconsidered or rescheduled.
    /// </summary>
    High,

    /// <summary>
    /// Critical severity - vacation cannot be approved as requested.
    /// </summary>
    Critical
}