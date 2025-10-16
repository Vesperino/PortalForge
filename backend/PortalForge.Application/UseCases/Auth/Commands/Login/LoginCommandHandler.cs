using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Common.Models;

namespace PortalForge.Application.UseCases.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResult>
{
    private readonly ISupabaseAuthService _authService;
    private readonly IUnifiedValidatorService _validatorService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        ISupabaseAuthService authService,
        IUnifiedValidatorService validatorService,
        ILogger<LoginCommandHandler> logger)
    {
        _authService = authService;
        _validatorService = validatorService;
        _logger = logger;
    }

    public async Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        await _validatorService.ValidateAsync(request);

        _logger.LogInformation("Login attempt for user: {Email}", request.Email);

        var authResult = await _authService.LoginAsync(
            request.Email,
            request.Password);

        if (!authResult.Success)
        {
            _logger.LogWarning("Login failed for user: {Email}", request.Email);
            throw new Exception(authResult.ErrorMessage ?? "Login failed");
        }

        _logger.LogInformation("User logged in successfully: {UserId}", authResult.UserId);

        return authResult;
    }
}
