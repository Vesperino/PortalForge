using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalForge.Api.DTOs.Requests.Admin;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.DTOs;
using PortalForge.Application.UseCases.Admin.Commands.AdjustVacationDays;
using PortalForge.Application.UseCases.Admin.Commands.ReseedRequestTemplates;
using PortalForge.Application.UseCases.Admin.Commands.SeedAdminData;
using PortalForge.Application.UseCases.Admin.Commands.SeedEmployees;
using PortalForge.Application.UseCases.Admin.Queries.GetAuditLogs;

namespace PortalForge.Api.Controllers;

/// <summary>
/// Controller for administrative operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AdminController : BaseController
{
    private readonly IMediator _mediator;
    private readonly ILogger<AdminController> _logger;
    private readonly ICurrentUserService _currentUserService;

    public AdminController(
        IMediator mediator,
        ILogger<AdminController> logger,
        ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    [HttpPost("seed")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<SeedAdminDataResult>> SeedData()
    {
        _logger.LogInformation("Seeding admin data...");

        var command = new SeedAdminDataCommand();
        var result = await _mediator.Send(command);

        return Ok(result);
    }

    /// <summary>
    /// Seed 40 sample employees with avatars
    /// </summary>
    [HttpPost("seed-employees")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<SeedEmployeesResult>> SeedEmployees()
    {
        _logger.LogInformation("Seeding 40 sample employees...");

        var command = new SeedEmployeesCommand();
        var result = await _mediator.Send(command);

        return Ok(result);
    }

    /// <summary>
    /// Force reseed of default request templates (vacation, sick leave).
    /// Removes old templates and creates new ones with updated structure.
    /// </summary>
    [HttpPost("reseed-request-templates")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ReseedResult>> ReseedRequestTemplates()
    {
        _logger.LogInformation("Reseeding default request templates...");

        var command = new ReseedRequestTemplatesCommand
        {
            ForceRecreate = true
        };
        var result = await _mediator.Send(command);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Manually adjust user's vacation days allowance.
    /// Used for corrections or special allowances.
    /// All adjustments are audited.
    /// </summary>
    /// <param name="userId">User ID to adjust vacation days for</param>
    /// <param name="adjustmentAmount">Amount to add (positive) or subtract (negative)</param>
    /// <param name="reason">Reason for adjustment (required for audit)</param>
    /// <returns>Result with old and new values</returns>
    [HttpPost("users/{userId}/adjust-vacation-days")]
    [Authorize(Policy = "HrOrAdmin")]
    public async Task<ActionResult<AdjustVacationDaysResult>> AdjustVacationDays(
        Guid userId,
        [FromBody] AdjustVacationDaysRequest request)
    {
        var command = new AdjustVacationDaysCommand
        {
            UserId = userId,
            AdjustmentAmount = request.AdjustmentAmount,
            Reason = request.Reason,
            AdjustedBy = _currentUserService.UserId
        };

        var result = await _mediator.Send(command);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Gets audit logs with filtering and pagination.
    /// </summary>
    /// <param name="entityType">Filter by entity type</param>
    /// <param name="action">Filter by action</param>
    /// <param name="userId">Filter by user ID</param>
    /// <param name="fromDate">Filter by minimum timestamp</param>
    /// <param name="toDate">Filter by maximum timestamp</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 50, max: 100)</param>
    /// <returns>Paginated audit log entries</returns>
    [HttpGet("audit-logs")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<PagedResult<AuditLogDto>>> GetAuditLogs(
        [FromQuery] string? entityType = null,
        [FromQuery] string? action = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = new GetAuditLogsQuery
        {
            EntityType = entityType,
            Action = action,
            UserId = userId,
            FromDate = fromDate,
            ToDate = toDate,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
