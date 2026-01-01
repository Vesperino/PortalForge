using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Common.Models;
using PortalForge.Application.Exceptions;
using PortalForge.Application.UseCases.Auth.DTOs;

namespace PortalForge.Application.UseCases.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResult>
{
    private readonly ISupabaseAuthService _authService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUnifiedValidatorService _validatorService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        ISupabaseAuthService authService,
        IUnitOfWork unitOfWork,
        IUnifiedValidatorService validatorService,
        ILogger<LoginCommandHandler> logger)
    {
        _authService = authService;
        _unitOfWork = unitOfWork;
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
            throw new ValidationException("Invalid email or password");
        }

        _logger.LogInformation("User logged in successfully: {UserId}", authResult.UserId);

        // Fetch full user data from database and map to DTO
        var user = await _unitOfWork.UserRepository.GetByIdAsync(authResult.UserId ?? Guid.Empty);

        if (user != null)
        {
            authResult.User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Department = user.Department,
                DepartmentId = user.DepartmentId,
                Position = user.Position,
                PositionId = user.PositionId,
                Role = user.Role.ToString().ToLower(),
                IsEmailVerified = user.IsEmailVerified,
                MustChangePassword = user.MustChangePassword,
                CreatedAt = user.CreatedAt
            };
        }

        return authResult;
    }
}
