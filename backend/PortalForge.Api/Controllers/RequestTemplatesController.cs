using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalForge.Application.UseCases.RequestTemplates.Commands.CreateRequestTemplate;
using PortalForge.Application.UseCases.RequestTemplates.Commands.DeleteRequestTemplate;
using PortalForge.Application.UseCases.RequestTemplates.Commands.SeedRequestTemplates;
using PortalForge.Application.UseCases.RequestTemplates.Commands.UpdateRequestTemplate;
using PortalForge.Application.UseCases.RequestTemplates.Queries.GetAvailableRequestTemplates;
using PortalForge.Application.UseCases.RequestTemplates.Queries.GetRequestTemplateById;
using PortalForge.Application.UseCases.RequestTemplates.Queries.GetRequestTemplates;

namespace PortalForge.Api.Controllers;

[Route("api/request-templates")]
[Authorize]
public class RequestTemplatesController : BaseController
{
    private readonly IMediator _mediator;
    private readonly ILogger<RequestTemplatesController> _logger;

    public RequestTemplatesController(IMediator mediator, ILogger<RequestTemplatesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all request templates (admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "RequirePermission:requests.manage_templates")]
    public async Task<ActionResult> GetAll()
    {
        var query = new GetRequestTemplatesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get available request templates for current user
    /// </summary>
    [HttpGet("available")]
    public async Task<ActionResult> GetAvailable()
    {
        var unauthorizedResult = GetUserIdOrUnauthorized(out var userGuid);
        if (unauthorizedResult != null)
        {
            return unauthorizedResult;
        }

        var query = new GetAvailableRequestTemplatesQuery { UserId = userGuid };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get request template by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(Guid id)
    {
        var query = new GetRequestTemplateByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
        if (result.Template == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Create new request template
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequirePermission:requests.manage_templates")]
    public async Task<ActionResult> Create([FromBody] CreateRequestTemplateCommand command)
    {
        var unauthorizedResult = GetUserIdOrUnauthorized(out var creatorId);
        if (unauthorizedResult != null)
        {
            return unauthorizedResult;
        }

        command.CreatedById = creatorId;
        var result = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update request template
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "RequirePermission:requests.manage_templates")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateRequestTemplateCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete request template
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "RequirePermission:requests.manage_templates")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var unauthorizedResult = GetUserIdOrUnauthorized(out var deletedBy);
        if (unauthorizedResult != null)
        {
            return unauthorizedResult;
        }

        var command = new DeleteRequestTemplateCommand
        {
            Id = id,
            DeletedBy = deletedBy
        };

        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Seed sample request templates (for testing/development)
    /// </summary>
    [HttpPost("seed")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Seed()
    {
        var command = new SeedRequestTemplatesCommand();
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}

