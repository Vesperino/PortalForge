using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;

namespace PortalForge.Application.UseCases.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly ISupabaseAuthService _authService;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(
        ISupabaseAuthService authService,
        ILogger<LogoutCommandHandler> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("User logout attempt");

        var result = await _authService.LogoutAsync(string.Empty);

        if (!result)
        {
            _logger.LogWarning("Logout failed");
            throw new BusinessException("Logout failed. Please try again.");
        }

        _logger.LogInformation("User logged out successfully");

        return Unit.Value;
    }
}
