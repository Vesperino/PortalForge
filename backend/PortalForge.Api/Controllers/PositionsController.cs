using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.DTOs;
using PortalForge.Application.UseCases.Positions.Commands.CreatePosition;
using PortalForge.Application.UseCases.Positions.Commands.DeletePosition;
using PortalForge.Application.UseCases.Positions.Commands.UpdatePosition;
using PortalForge.Application.UseCases.Positions.Queries.GetAllPositions;
using PortalForge.Application.UseCases.Positions.Queries.SearchPositions;

namespace PortalForge.Api.Controllers;

/// <summary>
/// Controller for managing job positions.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PositionsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly ILogger<PositionsController> _logger;

    public PositionsController(IMediator mediator, ILogger<PositionsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<GetAllPositionsResult>> GetAll(
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isActive = true,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetAllPositionsQuery
        {
            SearchTerm = searchTerm,
            IsActive = isActive,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Searches positions by name for autocomplete.
    /// </summary>
    /// <param name="searchTerm">Search term to filter positions</param>
    /// <returns>List of matching positions (max 10)</returns>
    [HttpGet("search")]
    [Authorize]
    public async Task<ActionResult<List<PositionDto>>> Search([FromQuery] string searchTerm = "")
    {
        var query = new SearchPositionsQuery { SearchTerm = searchTerm };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new position.
    /// </summary>
    /// <param name="command">Position creation data</param>
    /// <returns>ID of the created position</returns>
    [HttpPost]
    [Authorize(Policy = "HrOrAdmin")]
    public async Task<ActionResult<Guid>> Create([FromBody] CreatePositionCommand command)
    {
        var positionId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAll), new { id = positionId }, positionId);
    }

    /// <summary>
    /// Updates an existing position.
    /// </summary>
    /// <param name="id">Position ID</param>
    /// <param name="command">Position update data</param>
    /// <returns>No content on success</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "HrOrAdmin")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdatePositionCommand command)
    {
        command.PositionId = id;
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Deletes a position.
    /// </summary>
    /// <param name="id">Position ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "HrOrAdmin")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var command = new DeletePositionCommand { PositionId = id };
        await _mediator.Send(command);
        return NoContent();
    }
}
