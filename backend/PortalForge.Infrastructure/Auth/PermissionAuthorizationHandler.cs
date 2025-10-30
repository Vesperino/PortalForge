using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PortalForge.Infrastructure.Persistence;
using System.Security.Claims;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Auth;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<PermissionAuthorizationHandler> _logger;

    public PermissionAuthorizationHandler(
        ApplicationDbContext dbContext,
        ILogger<PermissionAuthorizationHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Get user ID from claims
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("No valid user ID claim found");
            return;
        }

        _logger.LogInformation("Checking permission '{Permission}' for user {UserId}", requirement.PermissionName, userId);

        // Check if user has Admin role - Admins have all permissions
        // Use FindAll because there might be multiple role claims (one from JWT, one from our app)
        var roleClaims = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        _logger.LogInformation("User role claims: {Roles}", string.Join(", ", roleClaims));

        if (roleClaims.Contains(UserRole.Admin.ToString()))
        {
            _logger.LogInformation("User has Admin role, granting permission");
            context.Succeed(requirement);
            return;
        }

        // Check if user has the required permission through their role groups
        var hasPermission = await _dbContext.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.UserRoleGroups)
            .SelectMany(urg => urg.RoleGroup.RoleGroupPermissions)
            .AnyAsync(rgp => rgp.Permission.Name == requirement.PermissionName);

        _logger.LogInformation("User has permission through role groups: {HasPermission}", hasPermission);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}

