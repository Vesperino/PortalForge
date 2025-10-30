using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalForge.Application.UseCases.Admin.Commands.CreateRoleGroup;
using PortalForge.Application.UseCases.Admin.Commands.UpdateRoleGroup;
using PortalForge.Application.UseCases.Admin.Commands.DeleteRoleGroup;
using PortalForge.Application.UseCases.Admin.Queries.GetRoleGroups;
using PortalForge.Application.UseCases.Admin.Queries.GetRoleGroupById;
using System.Security.Claims;

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

    [HttpGet("{id}")]
    public async Task<ActionResult<GetRoleGroupByIdResult>> GetRoleGroupById(Guid id)
    {
        _logger.LogInformation("Getting role group by ID: {RoleGroupId}", id);

        var query = new GetRoleGroupByIdQuery
        {
            RoleGroupId = id
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CreateRoleGroupResult>> CreateRoleGroup([FromBody] CreateRoleGroupRequest request)
    {
        _logger.LogInformation("Creating role group: {Name}", request.Name);

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var command = new CreateRoleGroupCommand
        {
            Name = request.Name,
            Description = request.Description,
            PermissionIds = request.PermissionIds,
            CreatedBy = userId
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetRoleGroupById), new { id = result.RoleGroupId }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UpdateRoleGroupResult>> UpdateRoleGroup(Guid id, [FromBody] UpdateRoleGroupRequest request)
    {
        _logger.LogInformation("Updating role group: {RoleGroupId}", id);

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var command = new UpdateRoleGroupCommand
        {
            RoleGroupId = id,
            Name = request.Name,
            Description = request.Description,
            PermissionIds = request.PermissionIds,
            UpdatedBy = userId
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<DeleteRoleGroupResult>> DeleteRoleGroup(Guid id)
    {
        _logger.LogInformation("Deleting role group: {RoleGroupId}", id);

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var command = new DeleteRoleGroupCommand
        {
            RoleGroupId = id,
            DeletedBy = userId
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }
}

public class CreateRoleGroupRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Guid> PermissionIds { get; set; } = new();
}

public class UpdateRoleGroupRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Guid> PermissionIds { get; set; } = new();
}

