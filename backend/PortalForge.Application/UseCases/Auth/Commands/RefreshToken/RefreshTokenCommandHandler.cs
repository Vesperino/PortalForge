using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Common.Models;

namespace PortalForge.Application.UseCases.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResult>
{
    private readonly ISupabaseAuthService _authService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        ISupabaseAuthService authService,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async Task<AuthResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing token refresh request");

        var result = await _authService.RefreshTokenAsync(request.RefreshToken);

        if (!result.Success)
        {
            _logger.LogWarning("Token refresh failed");
            throw new Exception(result.ErrorMessage ?? "Token refresh failed");
        }

        _logger.LogInformation("Token refreshed successfully");
        return result;
    }
}
