using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalForge.Api.DTOs.Requests.Locations;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.Locations.Commands.CreateCachedLocation;
using PortalForge.Application.UseCases.Locations.Commands.DeleteCachedLocation;
using PortalForge.Application.UseCases.Locations.Commands.GeocodeAddress;
using PortalForge.Application.UseCases.Locations.Queries.GetCachedLocations;

namespace PortalForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LocationsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly ILogger<LocationsController> _logger;
    private readonly ICurrentUserService _currentUserService;

    public LocationsController(
        IMediator mediator,
        ILogger<LocationsController> logger,
        ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get all cached locations
    /// </summary>
    [HttpGet("cached")]
    public async Task<ActionResult<List<CachedLocationDto>>> GetCachedLocations()
    {
        var query = new GetCachedLocationsQuery();
        var result = await _mediator.Send(query);
        return Ok(result.Locations);
    }

    /// <summary>
    /// Geocode an address using OpenStreetMap Nominatim API
    /// </summary>
    [HttpPost("geocode")]
    public async Task<ActionResult<GeocodeAddressResult>> GeocodeAddress([FromBody] GeocodeRequest request)
    {
        var command = new GeocodeAddressCommand
        {
            Address = request.Address
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Add a cached location (Admin only)
    /// </summary>
    [HttpPost("admin/cached")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<CreateCachedLocationResult>> AddCachedLocation([FromBody] CreateCachedLocationRequest request)
    {
        var command = new CreateCachedLocationCommand
        {
            Name = request.Name,
            Address = request.Address,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Type = request.Type,
            CreatedBy = _currentUserService.UserId
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Delete a cached location (Admin only)
    /// </summary>
    [HttpDelete("admin/cached/{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<DeleteCachedLocationResult>> DeleteCachedLocation(int id)
    {
        var command = new DeleteCachedLocationCommand { Id = id };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
