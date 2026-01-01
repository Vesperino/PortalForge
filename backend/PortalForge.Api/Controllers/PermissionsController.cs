using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalForge.Application.DTOs;
using PortalForge.Application.UseCases.Admin.Queries.GetPermissions;
using PortalForge.Application.UseCases.Permissions.Commands.UpdateOrganizationalPermission;
using PortalForge.Application.UseCases.Permissions.Queries.GetOrganizationalPermission;
using System.Security.Claims;

namespace PortalForge.Api.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize]
public class PermissionsController : BaseController
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

    /// <summary>
    /// Get organizational permission for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Organizational permission data</returns>
    /// <response code="200">Returns the user's organizational permissions</response>
    /// <response code="403">User is not authorized to view these permissions</response>
    /// <response code="404">User not found</response>
    [HttpGet("organizational/{userId}")]
    [ProducesResponseType(typeof(OrganizationalPermissionDto), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetOrganizationalPermission(Guid userId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty)
            {
                return Unauthorized();
            }

            // Check if user is Admin role
            var isAdmin = User.IsInRole("Admin");

            // Users can view their own permissions, admins can view any
            if (userId != currentUserId && !isAdmin)
            {
                _logger.LogWarning(
                    "User {CurrentUserId} attempted to view permissions for user {TargetUserId} without authorization",
                    currentUserId, userId);
                return Forbid();
            }

            var query = new GetOrganizationalPermissionQuery
            {
                UserId = userId
            };

            var result = await _mediator.Send(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting organizational permission for user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while retrieving permissions" });
        }
    }

    /// <summary>
    /// Update organizational permission for a user (Admin only)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="command">Permission update data</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Permissions updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="403">User is not authorized (Admin only)</response>
    /// <response code="404">User not found</response>
    [HttpPut("organizational/{userId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateOrganizationalPermission(
        Guid userId,
        [FromBody] UpdateOrganizationalPermissionCommand command)
    {
        try
        {
            // Set userId from route parameter
            command.UserId = userId;

            await _mediator.Send(command);

            _logger.LogInformation(
                "Organizational permission updated for user {UserId} by {AdminId}",
                userId, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            return NoContent();
        }
        catch (FluentValidation.ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed for updating organizational permission for user {UserId}", userId);
            return BadRequest(new { message = "Validation failed", errors = ex.Errors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating organizational permission for user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while updating permissions" });
        }
    }
}

