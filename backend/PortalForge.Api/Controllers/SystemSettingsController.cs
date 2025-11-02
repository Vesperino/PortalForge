using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalForge.Api.DTOs.Requests.SystemSettings;
using PortalForge.Application.Interfaces;
using PortalForge.Application.UseCases.SystemSettings.Commands.TestStorage;
using PortalForge.Application.UseCases.SystemSettings.Commands.UpdateSettings;
using PortalForge.Application.UseCases.SystemSettings.Queries.GetAllSettings;
using PortalForge.Application.UseCases.SystemSettings.Queries.GetSettingByKey;

namespace PortalForge.Api.Controllers;

[ApiController]
[Route("api/admin/system-settings")]
[Authorize(Roles = "Admin")]
public class SystemSettingsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SystemSettingsController> _logger;

    public SystemSettingsController(
        IMediator mediator,
        ILogger<SystemSettingsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all system settings
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<SystemSettingDto>>> GetAll()
    {
        var query = new GetAllSettingsQuery();
        var result = await _mediator.Send(query);
        return Ok(result.Settings);
    }

    /// <summary>
    /// Get a specific system setting by key
    /// </summary>
    [HttpGet("{key}")]
    public async Task<ActionResult<SystemSettingDto>> GetByKey(string key)
    {
        var query = new GetSettingByKeyQuery { Key = key };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Update system settings (batch update)
    /// </summary>
    [HttpPut]
    public async Task<ActionResult<UpdateSettingsResult>> UpdateSettings([FromBody] List<UpdateSettingRequest> updates)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var command = new UpdateSettingsCommand
        {
            Settings = updates.Select(u => new UpdateSettingDto
            {
                Key = u.Key,
                Value = u.Value
            }).ToList(),
            UpdatedBy = Guid.Parse(userIdClaim)
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Test if storage paths are accessible
    /// </summary>
    [HttpPost("test-storage")]
    public async Task<ActionResult<StorageTestResult>> TestStorage()
    {
        var command = new TestStorageCommand();
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
