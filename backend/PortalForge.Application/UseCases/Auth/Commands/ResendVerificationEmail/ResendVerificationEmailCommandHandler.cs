using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Auth.Commands.ResendVerificationEmail;

public class ResendVerificationEmailCommandHandler : IRequestHandler<ResendVerificationEmailCommand, bool>
{
    private readonly ISupabaseAuthService _authService;
    private readonly ILogger<ResendVerificationEmailCommandHandler> _logger;

    public ResendVerificationEmailCommandHandler(
        ISupabaseAuthService authService,
        ILogger<ResendVerificationEmailCommandHandler> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async Task<bool> Handle(ResendVerificationEmailCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Resending verification email to: {Email}", request.Email);

        var result = await _authService.ResendVerificationEmailAsync(request.Email);

        if (!result)
        {
            _logger.LogWarning("Failed to resend verification email to: {Email}", request.Email);
        }

        return result;
    }
}
