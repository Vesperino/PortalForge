using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalForge.Application.UseCases.Admin.Commands.SeedAdminData;
using PortalForge.Application.UseCases.Admin.Commands.SeedEmployees;

namespace PortalForge.Api.Controllers;

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
}

