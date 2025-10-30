using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalForge.Application.UseCases.Admin.Queries.GetPermissions;

namespace PortalForge.Api.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize]
public class PermissionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PermissionsController> _logger;

    public PermissionsController(IMediator mediator, ILogger<PermissionsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<GetPermissionsResult>> GetPermissions(
        [FromQuery] string? category)
    {
        _logger.LogInformation("Getting permissions");

        var query = new GetPermissionsQuery
        {
            Category = category
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }
}

