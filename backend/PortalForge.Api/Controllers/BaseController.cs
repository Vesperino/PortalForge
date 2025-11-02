using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PortalForge.Api.Controllers;

/// <summary>
/// Base controller providing common functionality for all API controllers.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Extracts and validates the current user's ID from the JWT token.
    /// </summary>
    /// <param name="userId">The parsed user ID if successful.</param>
    /// <returns>True if user ID was successfully extracted and parsed, false otherwise.</returns>
    protected bool TryGetUserId(out Guid userId)
    {
        userId = Guid.Empty;

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            return false;
        }

        return Guid.TryParse(userIdClaim, out userId);
    }

    /// <summary>
    /// Gets the current user's ID from the JWT token.
    /// Returns Unauthorized response if the user ID is not found or invalid.
    /// </summary>
    /// <param name="userId">The parsed user ID if successful.</param>
    /// <param name="errorMessage">Custom error message to return. Defaults to "User ID not found in token".</param>
    /// <returns>Null if successful, or an UnauthorizedObjectResult if user ID extraction fails.</returns>
    protected ActionResult? GetUserIdOrUnauthorized(out Guid userId, string? errorMessage = null)
    {
        if (TryGetUserId(out userId))
        {
            return null;
        }

        return Unauthorized(errorMessage ?? "User ID not found in token");
    }

    /// <summary>
    /// Gets the current user's ID from the JWT token, or null if not found.
    /// Use this when you need optional user ID (e.g., for anonymous endpoints that behave differently when authenticated).
    /// </summary>
    /// <returns>The user ID if found and valid, otherwise null.</returns>
    protected Guid? GetUserIdOrNull()
    {
        if (TryGetUserId(out var userId))
        {
            return userId;
        }

        return null;
    }
}
