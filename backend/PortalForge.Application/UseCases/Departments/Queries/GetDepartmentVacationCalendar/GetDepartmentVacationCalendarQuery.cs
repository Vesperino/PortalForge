using MediatR;

namespace PortalForge.Application.UseCases.Departments.Queries.GetDepartmentVacationCalendar;

/// <summary>
/// Query to get vacation calendar for a department (including subdepartments).
/// Shows all vacations in the specified date range.
/// </summary>
public class GetDepartmentVacationCalendarQuery : IRequest<List<VacationCalendarEntryDto>>
{
    public Guid DepartmentId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

/// <summary>
/// DTO representing a vacation entry in the calendar.
/// </summary>
public class VacationCalendarEntryDto
{
    public Guid VacationId { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DaysCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string LeaveType { get; set; } = string.Empty;
}
