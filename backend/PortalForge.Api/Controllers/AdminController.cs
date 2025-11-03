using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalForge.Application.DTOs;
using PortalForge.Application.UseCases.Admin.Commands.SeedAdminData;
using PortalForge.Application.UseCases.Admin.Commands.SeedEmployees;
using PortalForge.Application.UseCases.Admin.Queries.GetAuditLogs;

namespace PortalForge.Api.Controllers;

/// <summary>
/// Controller for administrative operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IMediator mediator, ILogger<AdminController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("seed")]
    [AllowAnonymous] // Temporarily allow anonymous access for initial setup
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
    [AllowAnonymous] // Temporarily allow anonymous access for seeding
    public async Task<ActionResult<SeedEmployeesResult>> SeedEmployees()
    {
        _logger.LogInformation("Seeding 40 sample employees...");

        var command = new SeedEmployeesCommand();
        var result = await _mediator.Send(command);

        return Ok(result);
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
    [Authorize(Roles = "Admin")]
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

