using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.Common.Behaviors;

public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AuthorizationBehavior<TRequest, TResponse>> _logger;

    public AuthorizationBehavior(
        ICurrentUserService currentUserService,
        ILogger<AuthorizationBehavior<TRequest, TResponse>> logger)
    {
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IRequireAuthorization authRequest)
        {
            return await next();
        }

        if (authRequest.RequireAuthenticatedUser && !_currentUserService.IsAuthenticated)
        {
            _logger.LogWarning("Unauthorized access attempt to {RequestType}", typeof(TRequest).Name);
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        if (authRequest.RequiredRoles.Length > 0)
        {
            var hasRequiredRole = authRequest.RequiredRoles
                .Any(role => _currentUserService.IsInRole(role));

            if (!hasRequiredRole)
            {
                _logger.LogWarning(
                    "User {UserId} attempted to access {RequestType} without required roles: {RequiredRoles}",
                    _currentUserService.UserId,
                    typeof(TRequest).Name,
                    string.Join(", ", authRequest.RequiredRoles));
                throw new UnauthorizedAccessException($"User does not have required roles: {string.Join(", ", authRequest.RequiredRoles)}");
            }
        }

        return await next();
    }
}
