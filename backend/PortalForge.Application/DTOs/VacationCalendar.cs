using PortalForge.Domain.Entities;

namespace PortalForge.Application.DTOs;

/// <summary>
/// Represents a team vacation calendar with vacations, statistics, and conflict alerts.
/// </summary>
public class VacationCalendar
{
    /// <summary>
    /// All vacations in the requested date range.
    /// </summary>
    public List<VacationSchedule> Vacations { get; set; } = new();

    /// <summary>
    /// Total number of active employees in the team/department.
    /// </summary>
    public int TeamSize { get; set; }

    /// <summary>
    /// Alerts for dates where team coverage is low (>30% on vacation).
    /// </summary>
    public List<VacationAlert> Alerts { get; set; } = new();

    /// <summary>
    /// Statistical summary of vacation data.
    /// </summary>
    public VacationStatistics Statistics { get; set; } = new();
}

/// <summary>
/// Alert for dates with insufficient team coverage.
/// </summary>
public class VacationAlert
{
    /// <summary>
    /// Date when the alert occurs.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Severity level of the alert.
    /// </summary>
    public AlertType Type { get; set; }

    /// <summary>
    /// Employees who are on vacation on this date.
    /// </summary>
    public List<User> AffectedEmployees { get; set; } = new();

    /// <summary>
    /// Percentage of team that is on vacation (0-100).
    /// </summary>
    public double CoveragePercent { get; set; }

    /// <summary>
    /// Human-readable alert message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Statistical summary of vacation data.
/// </summary>
public class VacationStatistics
{
    /// <summary>
    /// Number of employees currently on vacation (Status = Active).
    /// </summary>
    public int CurrentlyOnVacation { get; set; }

    /// <summary>
    /// Number of scheduled future vacations (Status = Scheduled).
    /// </summary>
    public int ScheduledVacations { get; set; }

    /// <summary>
    /// Total number of vacation days in the calendar range.
    /// </summary>
    public int TotalVacationDays { get; set; }

    /// <summary>
    /// Average vacation duration in days.
    /// </summary>
    public double AverageVacationDays { get; set; }

    /// <summary>
    /// Total team size.
    /// </summary>
    public int TeamSize { get; set; }

    /// <summary>
    /// Percentage of team currently available (not on vacation).
    /// </summary>
    public double CoveragePercent { get; set; }
}

/// <summary>
/// Alert severity levels for vacation coverage.
/// </summary>
public enum AlertType
{
    /// <summary>
    /// 30-49% of team on vacation - reduced coverage.
    /// </summary>
    COVERAGE_LOW,

    /// <summary>
    /// 50%+ of team on vacation - critical coverage shortage.
    /// </summary>
    COVERAGE_CRITICAL
}
