# Backend: Vacation Schedule System

**Module**: Vacation Management & Substitute Routing
**Status**: 📋 Planned
**Last Updated**: 2025-10-31

---

## Overview

The Vacation Schedule System automatically manages vacation coverage by:
1. **Automatic Schedule Creation**: When vacation request is approved, create VacationSchedule
2. **Substitute Routing**: Route approval requests to substitute when approver is on vacation
3. **Conflict Detection**: Alert managers when >30% of team is on vacation
4. **Status Management**: Daily job updates vacation statuses (Scheduled → Active → Completed)

---

## Domain Entity

### VacationSchedule

**File**: `backend/PortalForge.Domain/Entities/VacationSchedule.cs`

```csharp
namespace PortalForge.Domain.Entities;

/// <summary>
/// Represents a scheduled vacation with automatic substitute assignment.
/// Created automatically when a vacation request is approved.
/// </summary>
public class VacationSchedule
{
    public Guid Id { get; set; }

    // ===== WHO =====

    /// <summary>
    /// User who is on vacation.
    /// </summary>
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>
    /// User who substitutes during vacation (handles approvals, etc.).
    /// </summary>
    public Guid SubstituteUserId { get; set; }
    public User Substitute { get; set; } = null!;

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
    public Request SourceRequest { get; set; } = null!;

    // ===== STATUS =====

    /// <summary>
    /// Current status of the vacation.
    /// Scheduled → Active → Completed (via daily job)
    /// </summary>
    public VacationStatus Status { get; set; } = VacationStatus.Scheduled;

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
```

### VacationStatus Enum

```csharp
namespace PortalForge.Domain.Enums;

public enum VacationStatus
{
    /// <summary>
    /// Vacation is scheduled but hasn't started yet (StartDate > today).
    /// </summary>
    Scheduled,

    /// <summary>
    /// Vacation is currently active (StartDate <= today <= EndDate).
    /// </summary>
    Active,

    /// <summary>
    /// Vacation has ended (EndDate < today).
    /// </summary>
    Completed,

    /// <summary>
    /// Vacation was cancelled before it started.
    /// </summary>
    Cancelled
}
```

---

## Application Service

### VacationScheduleService

**File**: `backend/PortalForge.Application/Services/VacationScheduleService.cs`

```csharp
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.Services;

public interface IVacationScheduleService
{
    /// <summary>
    /// Creates vacation schedule from approved vacation request.
    /// Extracts start date, end date, and substitute from form data.
    /// </summary>
    Task CreateFromApprovedRequestAsync(Request vacationRequest);

    /// <summary>
    /// Gets the active substitute for a user (if they're on vacation right now).
    /// Returns null if user is not on vacation.
    /// </summary>
    Task<User?> GetActiveSubstituteAsync(Guid userId);

    /// <summary>
    /// Gets team vacation calendar with statistics and conflict alerts.
    /// </summary>
    Task<VacationCalendar> GetTeamCalendarAsync(
        Guid departmentId,
        DateTime startDate,
        DateTime endDate);

    /// <summary>
    /// Gets vacations where the user is the substitute.
    /// </summary>
    Task<List<VacationSchedule>> GetMySubstitutionsAsync(Guid userId);

    /// <summary>
    /// Daily job: Updates vacation statuses based on current date.
    /// - Scheduled → Active (if StartDate <= today)
    /// - Active → Completed (if EndDate < today)
    /// </summary>
    Task UpdateVacationStatusesAsync();
}

public class VacationScheduleService : IVacationScheduleService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<VacationScheduleService> _logger;

    public VacationScheduleService(
        ApplicationDbContext context,
        ILogger<VacationScheduleService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task CreateFromApprovedRequestAsync(Request vacationRequest)
    {
        // 1. Parse form data
        var formData = JsonSerializer.Deserialize<Dictionary<string, object>>(
            vacationRequest.FormData
        ) ?? throw new InvalidOperationException("Invalid form data");

        // 2. Extract vacation details
        var startDate = DateTime.Parse(formData["startDate"].ToString()!);
        var endDate = DateTime.Parse(formData["endDate"].ToString()!);
        var substituteId = Guid.Parse(formData["substitute"].ToString()!);

        // 3. Validate substitute is not the user themselves
        if (substituteId == vacationRequest.SubmittedById)
        {
            _logger.LogWarning("User {UserId} tried to set themselves as substitute", substituteId);
            throw new ValidationException("Nie możesz być własnym zastępcą");
        }

        // 4. Check if substitute is active
        var substitute = await _context.Users.FindAsync(substituteId);
        if (substitute == null || !substitute.IsActive)
        {
            throw new NotFoundException($"Zastępca {substituteId} nie istnieje lub jest nieaktywny");
        }

        // 5. Create vacation schedule
        var schedule = new VacationSchedule
        {
            UserId = vacationRequest.SubmittedById,
            StartDate = startDate.Date, // Ensure date only (no time)
            EndDate = endDate.Date,
            SubstituteUserId = substituteId,
            SourceRequestId = vacationRequest.Id,
            Status = VacationStatus.Scheduled,
            CreatedAt = DateTime.UtcNow
        };

        _context.VacationSchedules.Add(schedule);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Created vacation schedule for {UserId} from {StartDate} to {EndDate}, substitute: {SubstituteId}",
            schedule.UserId, schedule.StartDate, schedule.EndDate, schedule.SubstituteUserId
        );

        // 6. Send notification to substitute
        await _notificationService.NotifySubstituteAsync(substituteId, schedule);
    }

    public async Task<User?> GetActiveSubstituteAsync(Guid userId)
    {
        var now = DateTime.UtcNow.Date;

        var schedule = await _context.VacationSchedules
            .Include(v => v.Substitute)
            .FirstOrDefaultAsync(v =>
                v.UserId == userId
                && v.StartDate <= now
                && v.EndDate >= now
                && v.Status == VacationStatus.Active
            );

        if (schedule != null)
        {
            _logger.LogDebug(
                "User {UserId} is on vacation, substitute is {SubstituteId}",
                userId, schedule.SubstituteUserId
            );
        }

        return schedule?.Substitute;
    }

    public async Task<VacationCalendar> GetTeamCalendarAsync(
        Guid departmentId,
        DateTime startDate,
        DateTime endDate)
    {
        // 1. Get all vacations in date range for department
        var vacations = await _context.VacationSchedules
            .Include(v => v.User)
            .Include(v => v.Substitute)
            .Where(v =>
                v.User.DepartmentId == departmentId
                && v.EndDate >= startDate
                && v.StartDate <= endDate
                && v.Status != VacationStatus.Cancelled
            )
            .OrderBy(v => v.StartDate)
            .ToListAsync();

        // 2. Get team size
        var teamSize = await _context.Users
            .CountAsync(u => u.DepartmentId == departmentId && u.IsActive);

        // 3. Detect conflicts
        var alerts = DetectConflicts(vacations, teamSize, startDate, endDate);

        // 4. Calculate statistics
        var statistics = CalculateStatistics(vacations, teamSize);

        return new VacationCalendar
        {
            Vacations = vacations,
            TeamSize = teamSize,
            Alerts = alerts,
            Statistics = statistics
        };
    }

    private List<VacationAlert> DetectConflicts(
        List<VacationSchedule> vacations,
        int teamSize,
        DateTime rangeStart,
        DateTime rangeEnd)
    {
        var alerts = new List<VacationAlert>();

        if (teamSize == 0) return alerts;

        // Iterate through each date in range
        var currentDate = rangeStart.Date;
        while (currentDate <= rangeEnd.Date)
        {
            // Count how many people are on vacation on this date
            var onVacationCount = vacations.Count(v =>
                v.StartDate <= currentDate && v.EndDate >= currentDate
            );

            var coveragePercent = (double)onVacationCount / teamSize * 100;

            // Alert if >30% of team on vacation
            if (coveragePercent >= 30)
            {
                var affectedEmployees = vacations
                    .Where(v => v.StartDate <= currentDate && v.EndDate >= currentDate)
                    .Select(v => v.User)
                    .ToList();

                alerts.Add(new VacationAlert
                {
                    Date = currentDate,
                    Type = coveragePercent >= 50
                        ? AlertType.COVERAGE_CRITICAL
                        : AlertType.COVERAGE_LOW,
                    AffectedEmployees = affectedEmployees,
                    CoveragePercent = coveragePercent,
                    Message = $"⚠️ {onVacationCount}/{teamSize} pracowników na urlopie ({coveragePercent:F0}%)"
                });
            }

            currentDate = currentDate.AddDays(1);
        }

        // Group consecutive dates into date ranges
        return GroupConsecutiveAlerts(alerts);
    }

    private VacationStatistics CalculateStatistics(
        List<VacationSchedule> vacations,
        int teamSize)
    {
        var now = DateTime.UtcNow.Date;

        var currentlyOnVacation = vacations.Count(v => v.Status == VacationStatus.Active);
        var scheduledVacations = vacations.Count(v => v.Status == VacationStatus.Scheduled);
        var totalVacationDays = vacations.Sum(v => v.DaysCount);
        var averageVacationDays = vacations.Any()
            ? vacations.Average(v => v.DaysCount)
            : 0;

        return new VacationStatistics
        {
            CurrentlyOnVacation = currentlyOnVacation,
            ScheduledVacations = scheduledVacations,
            TotalVacationDays = totalVacationDays,
            AverageVacationDays = averageVacationDays,
            TeamSize = teamSize,
            CoveragePercent = teamSize > 0
                ? (double)(teamSize - currentlyOnVacation) / teamSize * 100
                : 100
        };
    }

    public async Task UpdateVacationStatusesAsync()
    {
        var now = DateTime.UtcNow.Date;
        var updated = 0;

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. Activate scheduled vacations (StartDate <= today)
            var toActivate = await _context.VacationSchedules
                .Where(v => v.Status == VacationStatus.Scheduled && v.StartDate <= now)
                .ToListAsync();

            foreach (var vacation in toActivate)
            {
                vacation.Status = VacationStatus.Active;
                _logger.LogInformation(
                    "Activated vacation for {UserId} - now on vacation until {EndDate}",
                    vacation.UserId, vacation.EndDate
                );
                updated++;

                // Notify substitute
                await _notificationService.NotifyVacationStartedAsync(
                    vacation.SubstituteUserId,
                    vacation
                );
            }

            // 2. Complete active vacations (EndDate < today)
            var toComplete = await _context.VacationSchedules
                .Where(v => v.Status == VacationStatus.Active && v.EndDate < now)
                .ToListAsync();

            foreach (var vacation in toComplete)
            {
                vacation.Status = VacationStatus.Completed;
                _logger.LogInformation(
                    "Completed vacation for {UserId}",
                    vacation.UserId
                );
                updated++;

                // Notify user vacation ended
                await _notificationService.NotifyVacationEndedAsync(
                    vacation.UserId,
                    vacation
                );
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Updated {Count} vacation statuses", updated);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating vacation statuses");
            throw;
        }
    }
}
```

---

## DTOs

### VacationCalendar

```csharp
public class VacationCalendar
{
    public List<VacationSchedule> Vacations { get; set; } = new();
    public int TeamSize { get; set; }
    public List<VacationAlert> Alerts { get; set; } = new();
    public VacationStatistics Statistics { get; set; } = new();
}
```

### VacationAlert

```csharp
public class VacationAlert
{
    public DateTime Date { get; set; }
    public AlertType Type { get; set; }
    public List<User> AffectedEmployees { get; set; } = new();
    public double CoveragePercent { get; set; }
    public string Message { get; set; } = string.Empty;
}

public enum AlertType
{
    COVERAGE_LOW,      // 30-49% on vacation
    COVERAGE_CRITICAL  // 50%+ on vacation
}
```

### VacationStatistics

```csharp
public class VacationStatistics
{
    public int CurrentlyOnVacation { get; set; }
    public int ScheduledVacations { get; set; }
    public int TotalVacationDays { get; set; }
    public double AverageVacationDays { get; set; }
    public int TeamSize { get; set; }
    public double CoveragePercent { get; set; }
}
```

---

## API Controller

### VacationSchedulesController

**File**: `backend/PortalForge.Api/Controllers/VacationSchedulesController.cs`

```csharp
[ApiController]
[Route("api/vacation-schedules")]
[Authorize]
public class VacationSchedulesController : ControllerBase
{
    private readonly IVacationScheduleService _vacationService;
    private readonly IVacationExportService _exportService;

    /// <summary>
    /// Get team vacation calendar for a specific month.
    /// Only managers can view team calendar.
    /// </summary>
    [HttpGet("team")]
    [Authorize(Policy = "ManagerOrAbove")]
    [ProducesResponseType(typeof(VacationCalendar), 200)]
    public async Task<IActionResult> GetTeamCalendar(
        [FromQuery] Guid? departmentId,
        [FromQuery] int year,
        [FromQuery] int month)
    {
        // If no departmentId provided, use current user's department
        var deptId = departmentId ?? CurrentUser.DepartmentId;

        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var calendar = await _vacationService.GetTeamCalendarAsync(deptId, startDate, endDate);

        return Ok(calendar);
    }

    /// <summary>
    /// Get vacations where I'm the substitute.
    /// </summary>
    [HttpGet("my-substitutions")]
    [ProducesResponseType(typeof(List<VacationScheduleDto>), 200)]
    public async Task<IActionResult> GetMySubstitutions()
    {
        var substitutions = await _vacationService.GetMySubstitutionsAsync(CurrentUser.Id);
        return Ok(substitutions);
    }

    /// <summary>
    /// Export team vacation calendar to PDF.
    /// </summary>
    [HttpGet("export/pdf")]
    [Authorize(Policy = "ManagerOrAbove")]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    public async Task<IActionResult> ExportToPdf(
        [FromQuery] Guid? departmentId,
        [FromQuery] int year,
        [FromQuery] int month)
    {
        var deptId = departmentId ?? CurrentUser.DepartmentId;
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var pdfBytes = await _exportService.ExportToPdfAsync(deptId, startDate, endDate);

        var fileName = $"kalendarz-urlopow-{year}-{month:D2}.pdf";

        return File(pdfBytes, "application/pdf", fileName);
    }

    /// <summary>
    /// Export team vacation calendar to Excel.
    /// </summary>
    [HttpGet("export/excel")]
    [Authorize(Policy = "ManagerOrAbove")]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    public async Task<IActionResult> ExportToExcel(
        [FromQuery] Guid? departmentId,
        [FromQuery] int year,
        [FromQuery] int month)
    {
        var deptId = departmentId ?? CurrentUser.DepartmentId;
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var excelBytes = await _exportService.ExportToExcelAsync(deptId, startDate, endDate);

        var fileName = $"kalendarz-urlopow-{year}-{month:D2}.xlsx";

        return File(
            excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName
        );
    }
}
```

---

## Background Job

### UpdateVacationStatusesJob

**File**: `backend/PortalForge.Infrastructure/BackgroundJobs/UpdateVacationStatusesJob.cs`

```csharp
using Microsoft.Extensions.Hosting;

namespace PortalForge.Infrastructure.BackgroundJobs;

/// <summary>
/// Background job that runs daily to update vacation statuses.
/// - Activates scheduled vacations when StartDate arrives
/// - Completes active vacations when EndDate passes
/// </summary>
public class UpdateVacationStatusesJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UpdateVacationStatusesJob> _logger;

    public UpdateVacationStatusesJob(
        IServiceProvider serviceProvider,
        ILogger<UpdateVacationStatusesJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("UpdateVacationStatusesJob started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Calculate time until next midnight (00:01)
                var now = DateTime.UtcNow;
                var tomorrow = now.Date.AddDays(1).AddMinutes(1);
                var delay = tomorrow - now;

                _logger.LogInformation(
                    "Next vacation status update in {Hours}h {Minutes}m",
                    delay.Hours, delay.Minutes
                );

                // Wait until next midnight
                await Task.Delay(delay, stoppingToken);

                // Update vacation statuses
                using (var scope = _serviceProvider.CreateScope())
                {
                    var vacationService = scope.ServiceProvider
                        .GetRequiredService<IVacationScheduleService>();

                    await vacationService.UpdateVacationStatusesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateVacationStatusesJob");
                // Wait 1 hour before retry
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        _logger.LogInformation("UpdateVacationStatusesJob stopped");
    }
}
```

**Registration in Program.cs**:
```csharp
builder.Services.AddHostedService<UpdateVacationStatusesJob>();
```

---

## Integration with Request Approval

### Trigger: When Vacation Request Approved

**File**: `backend/PortalForge.Application/UseCases/Requests/Commands/ApproveRequestStep/ApproveRequestStepCommandHandler.cs`

```csharp
public async Task<ApproveRequestStepResult> Handle(
    ApproveRequestStepCommand request,
    CancellationToken cancellationToken)
{
    // ... existing approval logic

    // Check if all steps approved
    var allApproved = request.ApprovalSteps.All(s => s.Status == ApprovalStepStatus.Approved);

    if (allApproved)
    {
        request.Status = RequestStatus.Approved;
        request.CompletedAt = DateTime.UtcNow;

        // 🔥 NEW: If vacation request, create vacation schedule
        if (request.RequestTemplate.Name.Contains("urlop", StringComparison.OrdinalIgnoreCase)
            || request.RequestTemplate.RequiresSubstituteSelection)
        {
            await _vacationScheduleService.CreateFromApprovedRequestAsync(request);
        }
    }

    await _context.SaveChangesAsync(cancellationToken);

    return new ApproveRequestStepResult { Success = true };
}
```

### Substitute Routing

**File**: `backend/PortalForge.Application/UseCases/Requests/Commands/SubmitRequest/SubmitRequestCommandHandler.cs`

```csharp
private async Task<User> GetEffectiveApproverAsync(User approver)
{
    // Check if approver is on vacation
    var substitute = await _vacationScheduleService.GetActiveSubstituteAsync(approver.Id);

    if (substitute != null)
    {
        _logger.LogInformation(
            "Approver {ApproverId} is on vacation, routing to substitute {SubstituteId}",
            approver.Id, substitute.Id
        );
        return substitute;
    }

    return approver;
}
```

---

## Export Services

### VacationExportService (PDF)

**Library**: QuestPDF

```csharp
public async Task<byte[]> ExportToPdfAsync(
    Guid departmentId,
    DateTime startDate,
    DateTime endDate)
{
    var calendar = await _vacationService.GetTeamCalendarAsync(
        departmentId, startDate, endDate
    );

    var department = await _context.Departments.FindAsync(departmentId);

    var document = Document.Create(container =>
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(2, Unit.Centimetre);

            // Header
            page.Header().Text($"Kalendarz urlopów - {department.Name}")
                .FontSize(20).Bold();

            // Statistics
            page.Content().Column(column =>
            {
                column.Item().Text($"Miesiąc: {startDate:MMMM yyyy}");
                column.Item().Text($"Zespół: {calendar.Statistics.TeamSize} osób");
                column.Item().Text($"Obecnie na urlopie: {calendar.Statistics.CurrentlyOnVacation}");

                // Vacation list table
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3); // Employee
                        columns.RelativeColumn(2); // Start Date
                        columns.RelativeColumn(2); // End Date
                        columns.RelativeColumn(1); // Days
                        columns.RelativeColumn(3); // Substitute
                    });

                    // Table content...
                });
            });

            // Footer
            page.Footer()
                .AlignCenter()
                .Text($"Wygenerowano: {DateTime.Now:dd.MM.yyyy HH:mm}");
        });
    });

    return document.GeneratePdf();
}
```

---

## Testing

### Unit Tests

```csharp
public class VacationScheduleServiceTests
{
    [Fact]
    public async Task GetActiveSubstitute_ReturnsSubstitute_WhenUserOnVacation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var substituteId = Guid.NewGuid();

        var vacation = new VacationSchedule
        {
            UserId = userId,
            SubstituteUserId = substituteId,
            StartDate = DateTime.Today.AddDays(-1),
            EndDate = DateTime.Today.AddDays(5),
            Status = VacationStatus.Active
        };

        _context.VacationSchedules.Add(vacation);
        await _context.SaveChangesAsync();

        // Act
        var substitute = await _service.GetActiveSubstituteAsync(userId);

        // Assert
        substitute.Should().NotBeNull();
        substitute!.Id.Should().Be(substituteId);
    }

    [Fact]
    public async Task DetectConflicts_AlertsWhenMoreThan30PercentOnVacation()
    {
        // Arrange
        var teamSize = 10;
        var vacations = new List<VacationSchedule>
        {
            new() { StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(5) },
            new() { StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(5) },
            new() { StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(5) },
            new() { StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(5) }
            // 4 out of 10 = 40%
        };

        // Act
        var alerts = _service.DetectConflicts(vacations, teamSize);

        // Assert
        alerts.Should().NotBeEmpty();
        alerts[0].Type.Should().Be(AlertType.COVERAGE_LOW);
        alerts[0].CoveragePercent.Should().Be(40);
    }
}
```

---

## Performance Considerations

1. **Index on date ranges**: `IX_VacationSchedules_StartDate_EndDate`
2. **Cache team calendar**: Cache for 5 minutes (rarely changes)
3. **Optimize conflict detection**: Pre-calculate for common date ranges

---

## Security

1. **Only managers** can view team vacation calendar
2. **Users can only** see their own substitutions
3. **Validate substitute** is active user when creating schedule
4. **Prevent self-substitution**: User cannot be their own substitute
