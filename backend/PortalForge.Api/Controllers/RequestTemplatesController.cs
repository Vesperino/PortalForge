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
using System.Security.Claims;

namespace PortalForge.Api.Controllers;

[ApiController]
[Route("api/request-templates")]
[Authorize]
public class RequestTemplatesController : ControllerBase
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
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized("User ID not found in token");
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
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var creatorId))
        {
            return Unauthorized("User ID not found in token");
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
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var deletedBy))
        {
            return Unauthorized("User ID not found in token");
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
    [AllowAnonymous] // Allow anonymous access for testing/development
    public async Task<ActionResult> Seed()
    {
        var command = new SeedRequestTemplatesCommand();
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}

