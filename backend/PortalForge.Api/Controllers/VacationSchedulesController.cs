using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Api.DTOs.Requests.Vacations;
using PortalForge.Application.DTOs;
using PortalForge.Application.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;
using System.Security.Claims;
using System.Text.Json;

namespace PortalForge.Api.Controllers;

/// <summary>
/// Controller for managing vacation schedules and team calendar.
/// </summary>
[ApiController]
[Route("api/vacation-schedules")]
public class VacationSchedulesController : BaseController
{
    private readonly IVacationSubstituteService _vacationSubstituteService;
    private readonly IVacationStatusService _vacationStatusService;
    private readonly IVacationCalculationService _vacationCalculationService;
    private readonly IMediator _mediator;
    private readonly ILogger<VacationSchedulesController> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public VacationSchedulesController(
        IVacationSubstituteService vacationSubstituteService,
        IVacationStatusService vacationStatusService,
        IVacationCalculationService vacationCalculationService,
        IMediator mediator,
        ILogger<VacationSchedulesController> logger,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _vacationSubstituteService = vacationSubstituteService;
        _vacationStatusService = vacationStatusService;
        _vacationCalculationService = vacationCalculationService;
        _mediator = mediator;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Checks if current user has permission to view a specific department's data.
    /// </summary>
    private async Task<bool> CanViewDepartmentAsync(Guid departmentId)
    {
        var userId = _currentUserService.UserId;
        if (userId == Guid.Empty)
        {
            return false;
        }

        // Get user
        var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        // Admin and HR can view all departments
        if (user.Role == UserRole.Admin || user.Role == UserRole.HR)
        {
            _logger.LogInformation("User {UserId} with role {Role} granted access to department {DepartmentId}",
                userId, user.Role, departmentId);
            return true;
        }

        // User can always view their own department
        if (user.DepartmentId == departmentId)
        {
            _logger.LogInformation("User {UserId} viewing their own department {DepartmentId}",
                userId, departmentId);
            return true;
        }

        // Check organizational permissions
        var orgPermission = await _unitOfWork.OrganizationalPermissionRepository
            .GetByUserIdAsync(userId);

        if (orgPermission != null)
        {
            // User has permission to view all departments
            if (orgPermission.CanViewAllDepartments)
            {
                _logger.LogInformation("User {UserId} has CanViewAllDepartments permission",
                    userId);
                return true;
            }

            // Check if department is in visible list
            var visibleDepartmentIds = JsonSerializer.Deserialize<List<Guid>>(orgPermission.VisibleDepartmentIds);
            if (visibleDepartmentIds != null && visibleDepartmentIds.Contains(departmentId))
            {
                _logger.LogInformation("User {UserId} has explicit permission to view department {DepartmentId}",
                    userId, departmentId);
                return true;
            }
        }

        _logger.LogWarning("User {UserId} denied access to department {DepartmentId}",
            userId, departmentId);
        return false;
    }

    /// <summary>
    /// Validates if user can take vacation on specified dates.
    /// Used by frontend to show real-time feedback before submitting request.
    /// </summary>
    /// <param name="request">Vacation validation request</param>
    /// <returns>Validation result with error message if invalid</returns>
    [HttpPost("validate")]
    [Authorize]
    [ProducesResponseType(typeof(ValidateVacationResponse), 200)]
    public async Task<ActionResult<ValidateVacationResponse>> ValidateVacation(
        [FromBody] ValidateVacationRequest request)
    {
        _logger.LogInformation(
            "Validating vacation for user {UserId}: {LeaveType} from {StartDate} to {EndDate}",
            _currentUserService.UserId, request.LeaveType, request.StartDate, request.EndDate);

        var (canTake, errorMessage) = await _vacationCalculationService.CanTakeVacationAsync(
            _currentUserService.UserId,
            request.StartDate,
            request.EndDate,
            request.LeaveType);

        var businessDays = _vacationCalculationService.CalculateBusinessDays(
            request.StartDate,
            request.EndDate);

        return Ok(new ValidateVacationResponse
        {
            CanTake = canTake,
            ErrorMessage = errorMessage,
            RequestedDays = businessDays
        });
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

        // Check if user has permission to view this department's calendar
        if (!await CanViewDepartmentAsync(departmentId.Value))
        {
            _logger.LogWarning(
                "User {UserId} attempted to access department {DepartmentId} calendar without permission",
                _currentUserService.UserId, departmentId);
            return Forbid();
        }

        // Calculate date range for the month
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        _logger.LogInformation(
            "Getting team calendar for department {DepartmentId}, {Year}-{Month:D2}",
            departmentId, year, month);

        var calendar = await _vacationSubstituteService.GetTeamCalendarAsync(
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

        var substitutions = await _vacationSubstituteService.GetMySubstitutionsAsync(userId);

        return Ok(substitutions);
    }

    /// <summary>
    /// Gets vacations for the current user (all statuses), optionally filtered by year.
    /// </summary>
    [HttpGet("my")]
    [Authorize]
    [ProducesResponseType(typeof(List<VacationSchedule>), 200)]
    public async Task<ActionResult<List<VacationSchedule>>> GetMyVacations([FromQuery] int? year)
    {
        var userId = _currentUserService.UserId;

        _logger.LogInformation("Getting my vacations for user {UserId} (year={Year})", userId, year);
        var all = await _unitOfWork.VacationScheduleRepository.GetByUserAsync(userId);

        if (year.HasValue)
        {
            var y = year.Value;
            all = all.Where(v => v.StartDate.Year == y || v.EndDate.Year == y).ToList();
        }

        return Ok(all);
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
            vacationId, _currentUserService.UserId);

        var command = new Application.UseCases.Vacations.Commands.CancelVacation.CancelVacationCommand
        {
            VacationScheduleId = vacationId,
            CancelledByUserId = _currentUserService.UserId,
            Reason = request.Reason
        };

        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Manually triggers vacation status updates (Scheduled → Active, Active → Completed).
    /// Admin only endpoint for maintenance purposes.
    /// </summary>
    /// <returns>Number of vacations updated</returns>
    [HttpPost("update-statuses")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(UpdateStatusesResponse), 200)]
    public async Task<ActionResult<UpdateStatusesResponse>> UpdateVacationStatuses(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Manual vacation status update triggered by user {UserId}", _currentUserService.UserId);

        await _vacationStatusService.UpdateVacationStatusesAsync(cancellationToken);

        return Ok(new UpdateStatusesResponse
        {
            Message = "Vacation statuses updated successfully"
        });
    }
}




