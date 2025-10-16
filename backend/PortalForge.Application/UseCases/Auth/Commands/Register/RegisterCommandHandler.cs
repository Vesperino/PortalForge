using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;

namespace PortalForge.Application.UseCases.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResult>
{
    private readonly ISupabaseAuthService _authService;
    private readonly IUnifiedValidatorService _validatorService;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        ISupabaseAuthService authService,
        IUnifiedValidatorService validatorService,
        ILogger<RegisterCommandHandler> logger)
    {
        _authService = authService;
        _validatorService = validatorService;
        _logger = logger;
    }

    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Registering new user: {Email}, Name: {FirstName} {LastName}",
            request.Email, request.FirstName, request.LastName);

        await _validatorService.ValidateAsync(request);

        var authResult = await _authService.RegisterAsync(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName);

        if (!authResult.Success)
        {
            _logger.LogWarning(
                "Registration failed for {Email}: {Message}",
                request.Email, authResult.ErrorMessage);

            throw new ValidationException(
                "Registration failed",
                new List<string> { authResult.ErrorMessage ?? "Unknown error occurred" });
        }

        _logger.LogInformation(
            "User registered successfully: {UserId}, Email: {Email}",
            authResult.UserId, request.Email);

        return new RegisterResult
        {
            UserId = authResult.UserId ?? Guid.Empty,
            Email = request.Email,
            Message = "Registration successful. Please check your email to verify your account."
        };
    }
}
