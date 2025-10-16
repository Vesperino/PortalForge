using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Auth.Commands.VerifyEmail;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Unit>
{
    private readonly ISupabaseAuthService _authService;
    private readonly ILogger<VerifyEmailCommandHandler> _logger;

    public VerifyEmailCommandHandler(
        ISupabaseAuthService authService,
        ILogger<VerifyEmailCommandHandler> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async Task<Unit> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Email verification attempt for: {Email}", request.Email);

        var result = await _authService.VerifyEmailAsync(request.Token);

        if (!result)
        {
            _logger.LogWarning("Email verification failed for: {Email}", request.Email);
            throw new Exception("Email verification failed");
        }

        _logger.LogInformation("Email verified successfully for: {Email}", request.Email);

        return Unit.Value;
    }
}
