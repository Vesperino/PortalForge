using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalForge.Application.UseCases.Admin.Commands.CreateUser;
using PortalForge.Application.UseCases.Admin.Commands.DeleteUser;
using PortalForge.Application.UseCases.Admin.Commands.UpdateUser;
using PortalForge.Application.UseCases.Admin.Queries.GetUserById;
using PortalForge.Application.UseCases.Admin.Queries.GetUsers;
using System.Security.Claims;

namespace PortalForge.Api.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IMediator mediator, ILogger<UsersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    [HttpGet]
    public async Task<ActionResult<GetUsersResult>> GetUsers(
        [FromQuery] string? searchTerm,
        [FromQuery] string? department,
        [FromQuery] string? position,
        [FromQuery] string? role,
        [FromQuery] bool? isActive,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = "CreatedAt",
        [FromQuery] bool sortDescending = true)
    {
        _logger.LogInformation("Getting users list");

        var query = new GetUsersQuery
        {
            SearchTerm = searchTerm,
            Department = department,
            Position = position,
            Role = role,
            IsActive = isActive,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(Guid id)
    {
        _logger.LogInformation("Getting user by ID: {UserId}", id);

        var query = new GetUserByIdQuery { UserId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CreateUserResult>> CreateUser([FromBody] CreateUserRequest request)
    {
        _logger.LogInformation("Creating new user: {Email}", request.Email);

        var command = new CreateUserCommand
        {
            Email = request.Email,
            Password = request.Password,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Department = request.Department,
            Position = request.Position,
            PhoneNumber = request.PhoneNumber,
            Role = request.Role,
            RoleGroupIds = request.RoleGroupIds,
            MustChangePassword = request.MustChangePassword,
            CreatedBy = GetCurrentUserId()
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetUserById), new { id = result.UserId }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UpdateUserResult>> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        _logger.LogInformation("Updating user: {UserId}", id);

        var command = new UpdateUserCommand
        {
            UserId = id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Department = request.Department,
            Position = request.Position,
            PhoneNumber = request.PhoneNumber,
            Role = request.Role,
            RoleGroupIds = request.RoleGroupIds,
            IsActive = request.IsActive,
            UpdatedBy = GetCurrentUserId()
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<DeleteUserResult>> DeleteUser(Guid id)
    {
        _logger.LogInformation("Deleting user: {UserId}", id);

        var command = new DeleteUserCommand
        {
            UserId = id,
            DeletedBy = GetCurrentUserId()
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }
}

public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = "Employee";
    public List<Guid> RoleGroupIds { get; set; } = new();
    public bool MustChangePassword { get; set; } = true;
}

public class UpdateUserRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = string.Empty;
    public List<Guid> RoleGroupIds { get; set; } = new();
    public bool IsActive { get; set; }
}

