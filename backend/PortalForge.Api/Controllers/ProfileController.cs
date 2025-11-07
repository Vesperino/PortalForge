using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalForge.Application.UseCases.Profile.Commands.UpdateMyProfile;
using System.Security.Claims;

namespace PortalForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(IMediator mediator, ILogger<ProfileController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    /// <summary>
    /// Update current user's profile (phone number and profile photo)
    /// </summary>
    [HttpPut]
    public async Task<ActionResult<UpdateMyProfileResult>> UpdateMyProfile([FromBody] UpdateMyProfileRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        _logger.LogInformation("User {UserId} updating their profile", userId);

        var command = new UpdateMyProfileCommand
        {
            UserId = userId,
            PhoneNumber = request.PhoneNumber,
            ProfilePhotoUrl = request.ProfilePhotoUrl
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }
}

public class UpdateMyProfileRequest
{
    public string? PhoneNumber { get; set; }
    public string? ProfilePhotoUrl { get; set; }
}
