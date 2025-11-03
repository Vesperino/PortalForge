using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalForge.Application.DTOs;
using PortalForge.Application.Services;
using PortalForge.Domain.Entities;
using System.Security.Claims;

namespace PortalForge.Api.Controllers;

/// <summary>
/// Controller for managing vacation schedules and team calendar.
/// </summary>
[ApiController]
[Route("api/vacation-schedules")]
public class VacationSchedulesController : ControllerBase
{
    private readonly IVacationScheduleService _vacationService;
    private readonly IMediator _mediator;
    private readonly ILogger<VacationSchedulesController> _logger;

    public VacationSchedulesController(
        IVacationScheduleService vacationService,
        IMediator mediator,
        ILogger<VacationSchedulesController> logger)
    {
        _vacationService = vacationService;
        _mediator = mediator;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    /// <summary>
    /// Gets team vacation calendar for a specific department and date range.
    /// </summary>
    /// <param name="departmentId">Department ID (optional - defaults to user's department)</param>
    /// <param name="year">Year (required)</param>
    /// <param name="month">Month 1-12 (required)</param>
    /// <returns>Vacation calendar with statistics and alerts</returns>
    [HttpGet("team")]
    [Authorize]
    [ProducesResponseType(typeof(VacationCalendar), 200)]
    public async Task<ActionResult<VacationCalendar>> GetTeamCalendar(
        [FromQuery] Guid? departmentId,
        [FromQuery] int year,
        [FromQuery] int month)
    {
        // Validate month
        if (month < 1 || month > 12)
        {
            return BadRequest("Month must be between 1 and 12");
        }

        // Validate year
        if (year < 2000 || year > 2100)
        {
            return BadRequest("Year must be between 2000 and 2100");
        }

        // If no department specified, would need to get user's department
        // For now, require departmentId
        if (!departmentId.HasValue)
        {
            return BadRequest("DepartmentId is required");
        }

        // Calculate date range for the month
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        _logger.LogInformation(
            "Getting team calendar for department {DepartmentId}, {Year}-{Month:D2}",
            departmentId, year, month);

        var calendar = await _vacationService.GetTeamCalendarAsync(
            departmentId.Value,
            startDate,
            endDate);

        return Ok(calendar);
    }

    /// <summary>
    /// Gets vacations where the current user is the substitute.
    /// </summary>
    /// <returns>List of vacations where user is covering for someone</returns>
    [HttpGet("my-substitutions")]
    [Authorize]
    [ProducesResponseType(typeof(List<VacationSchedule>), 200)]
    public async Task<ActionResult<List<VacationSchedule>>> GetMySubstitutions()
    {
        // Get current user ID from token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("User ID not found in token");
        }

        _logger.LogInformation("Getting substitutions for user {UserId}", userId);

        var substitutions = await _vacationService.GetMySubstitutionsAsync(userId);

        return Ok(substitutions);
    }

    /// <summary>
    /// Exports team vacation calendar to PDF.
    /// </summary>
    /// <param name="departmentId">Department ID</param>
    /// <param name="year">Year</param>
    /// <param name="month">Month 1-12</param>
    /// <returns>PDF file download</returns>
    [HttpGet("export/pdf")]
    [Authorize]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    public async Task<IActionResult> ExportToPdf(
        [FromQuery] Guid departmentId,
        [FromQuery] int year,
        [FromQuery] int month)
    {
        // TODO: Implement PDF export using QuestPDF
        // This requires adding QuestPDF NuGet package and creating a PDF generation service
        // For MVP, returning NotImplemented status

        _logger.LogWarning(
            "PDF export requested but not yet implemented for department {DepartmentId}, {Year}-{Month:D2}",
            departmentId, year, month);

        return StatusCode(
            StatusCodes.Status501NotImplemented,
            new { message = "PDF export feature will be implemented in future release. Use Excel export as alternative." });
    }

    /// <summary>
    /// Exports team vacation calendar to Excel.
    /// </summary>
    /// <param name="departmentId">Department ID</param>
    /// <param name="year">Year</param>
    /// <param name="month">Month 1-12</param>
    /// <returns>Excel file download</returns>
    [HttpGet("export/excel")]
    [Authorize]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    public async Task<IActionResult> ExportToExcel(
        [FromQuery] Guid departmentId,
        [FromQuery] int year,
        [FromQuery] int month)
    {
        // TODO: Implement Excel export using EPPlus or ClosedXML
        // This requires adding EPPlus/ClosedXML NuGet package and creating an Excel generation service
        // For MVP, returning NotImplemented status

        _logger.LogWarning(
            "Excel export requested but not yet implemented for department {DepartmentId}, {Year}-{Month:D2}",
            departmentId, year, month);

        return StatusCode(
            StatusCodes.Status501NotImplemented,
            new { message = "Excel export feature will be implemented in future release." });
    }

    /// <summary>
    /// Cancels an active vacation schedule.
    /// Admin can cancel anytime. Approvers can cancel up to 1 day after vacation starts.
    /// </summary>
    /// <param name="vacationId">Vacation schedule ID</param>
    /// <param name="request">Cancellation request with reason</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{vacationId:guid}")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CancelVacation(
        Guid vacationId,
        [FromBody] CancelVacationRequest request)
    {
        _logger.LogInformation(
            "Cancelling vacation {VacationId} by user {UserId}",
            vacationId, GetCurrentUserId());

        var command = new Application.UseCases.Vacations.Commands.CancelVacation.CancelVacationCommand
        {
            VacationScheduleId = vacationId,
            CancelledByUserId = GetCurrentUserId(),
            Reason = request.Reason
        };

        await _mediator.Send(command);
        return NoContent();
    }
}

public class CancelVacationRequest
{
    public string Reason { get; set; } = string.Empty;
}
