using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalForge.Api.DTOs.Requests.RoleGroups;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.Admin.Commands.CreateRoleGroup;
using PortalForge.Application.UseCases.Admin.Commands.UpdateRoleGroup;
using PortalForge.Application.UseCases.Admin.Commands.DeleteRoleGroup;
using PortalForge.Application.UseCases.Admin.Queries.GetRoleGroups;
using PortalForge.Application.UseCases.Admin.Queries.GetRoleGroupById;

namespace PortalForge.Api.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize]
public class RoleGroupsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly ILogger<RoleGroupsController> _logger;
    private readonly ICurrentUserService _currentUserService;

    public RoleGroupsController(
        IMediator mediator,
        ILogger<RoleGroupsController> logger,
        ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _logger = logger;
        _currentUserService = currentUserService;
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

        var command = new CreateRoleGroupCommand
        {
            Name = request.Name,
            Description = request.Description,
            PermissionIds = request.PermissionIds,
            CreatedBy = _currentUserService.UserId
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetRoleGroupById), new { id = result.RoleGroupId }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UpdateRoleGroupResult>> UpdateRoleGroup(Guid id, [FromBody] UpdateRoleGroupRequest request)
    {
        _logger.LogInformation("Updating role group: {RoleGroupId}", id);

        var command = new UpdateRoleGroupCommand
        {
            RoleGroupId = id,
            Name = request.Name,
            Description = request.Description,
            PermissionIds = request.PermissionIds,
            UpdatedBy = _currentUserService.UserId
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<DeleteRoleGroupResult>> DeleteRoleGroup(Guid id)
    {
        _logger.LogInformation("Deleting role group: {RoleGroupId}", id);

        var command = new DeleteRoleGroupCommand
        {
            RoleGroupId = id,
            DeletedBy = _currentUserService.UserId
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
