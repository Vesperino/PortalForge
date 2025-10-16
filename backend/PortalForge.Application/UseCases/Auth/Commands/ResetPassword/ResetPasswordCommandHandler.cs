using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
{
    private readonly ISupabaseAuthService _authService;
    private readonly IUnifiedValidatorService _validatorService;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(
        ISupabaseAuthService authService,
        IUnifiedValidatorService validatorService,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _authService = authService;
        _validatorService = validatorService;
        _logger = logger;
    }

    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        await _validatorService.ValidateAsync(request);

        _logger.LogInformation("Password reset requested for: {Email}", request.Email);

        var result = await _authService.SendPasswordResetEmailAsync(request.Email);

        if (!result)
        {
            _logger.LogWarning("Password reset failed for: {Email}", request.Email);
            throw new Exception("Failed to send password reset email");
        }

        _logger.LogInformation("Password reset email sent to: {Email}", request.Email);

        return Unit.Value;
    }
}
