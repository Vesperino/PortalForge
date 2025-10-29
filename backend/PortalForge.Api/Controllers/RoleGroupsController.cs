using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalForge.Application.UseCases.Admin.Queries.GetRoleGroups;

namespace PortalForge.Api.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize]
public class RoleGroupsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<RoleGroupsController> _logger;

    public RoleGroupsController(IMediator mediator, ILogger<RoleGroupsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<GetRoleGroupsResult>> GetRoleGroups(
        [FromQuery] bool includePermissions = true)
    {
        _logger.LogInformation("Getting role groups");

        var query = new GetRoleGroupsQuery
        {
            IncludePermissions = includePermissions
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }
}

