using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalForge.Application.DTOs;
using PortalForge.Application.UseCases.Users.Queries.SearchUsers;

namespace PortalForge.Api.Controllers;

/// <summary>
/// Public controller for user-related operations accessible to all authenticated users.
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize]
public class PublicUsersController : BaseController
{
    private readonly IMediator _mediator;
    private readonly ILogger<PublicUsersController> _logger;

    public PublicUsersController(IMediator mediator, ILogger<PublicUsersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Search users by name, email, department, or position. Used for autocomplete components.
    /// </summary>
    /// <param name="q">Search query (minimum 2 characters)</param>
    /// <param name="departmentId">Optional department filter</param>
    /// <param name="limit">Maximum number of results (default 10)</param>
    /// <returns>List of matching users</returns>
    [HttpGet("search")]
    public async Task<ActionResult<List<UserSearchDto>>> SearchUsers(
        [FromQuery] string q,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
        {
            return Ok(new List<UserSearchDto>());
        }

        var query = new SearchUsersQuery
        {
            Query = q,
            DepartmentId = departmentId,
            Limit = Math.Min(limit, 50), // Max 50 results
            OnlyActive = true
        };

        var results = await _mediator.Send(query);
        return Ok(results);
    }
}
