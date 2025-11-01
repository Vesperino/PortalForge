using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Common.Models;
using PortalForge.Application.UseCases.Auth.Commands.ChangePassword;
using PortalForge.Application.UseCases.Auth.Commands.Login;
using PortalForge.Application.UseCases.Auth.Commands.Logout;
using PortalForge.Application.UseCases.Auth.Commands.RefreshToken;
using PortalForge.Application.UseCases.Auth.Commands.Register;
using PortalForge.Application.UseCases.Auth.Commands.ResendVerificationEmail;
using PortalForge.Application.UseCases.Auth.Commands.ResetPassword;
using PortalForge.Application.UseCases.Auth.Commands.VerifyEmail;
using PortalForge.Application.UseCases.Auth.DTOs;
using PortalForge.Application.UseCases.Auth.Queries.GetCurrentUser;

namespace PortalForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _environment;

    public AuthController(
        IMediator mediator,
        ILogger<AuthController> logger,
        IUnitOfWork unitOfWork,
        IWebHostEnvironment environment)
    {
        _mediator = mediator;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _environment = environment;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<RegisterResult>> Register([FromBody] RegisterRequestDto request)
    {
        var command = new RegisterCommand
        {
            Email = request.Email,
            Password = request.Password,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Department = "Unassigned",
            Position = "Unassigned"
        };

        var result = await _mediator.Send(command);

        return Ok(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        var command = new LoginCommand
        {
            Email = request.Email,
            Password = request.Password
        };

        var result = await _mediator.Send(command);

        // Get full user data from database
        var user = await _unitOfWork.UserRepository.GetByIdAsync(result.UserId ?? Guid.Empty);

        if (user == null)
        {
            return BadRequest("User not found");
        }

        var response = new AuthResponseDto
        {
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Department = user.Department,
                DepartmentId = user.DepartmentId,
                Position = user.Position,
                Role = user.Role.ToString().ToLower(),
                IsEmailVerified = user.IsEmailVerified,
                MustChangePassword = user.MustChangePassword,
                CreatedAt = user.CreatedAt
            },
            AccessToken = result.AccessToken ?? string.Empty,
            RefreshToken = result.RefreshToken ?? string.Empty
        };

        SetRefreshTokenCookie(result.RefreshToken ?? string.Empty);

        return Ok(response);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout()
    {
        await _mediator.Send(new LogoutCommand());

        Response.Cookies.Delete("refresh_token");

        return Ok(new { message = "Wylogowano pomyślnie" });
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<RefreshTokenResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        var command = new RefreshTokenCommand
        {
            RefreshToken = request.RefreshToken
        };

        var result = await _mediator.Send(command);

        var response = new RefreshTokenResponseDto
        {
            AccessToken = result.AccessToken ?? string.Empty,
            RefreshToken = result.RefreshToken ?? string.Empty,
            ExpiresAt = result.ExpiresAt
        };

        // Update refresh token cookie
        if (!string.IsNullOrEmpty(result.RefreshToken))
        {
            SetRefreshTokenCookie(result.RefreshToken);
        }

        return Ok(response);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        var command = new ResetPasswordCommand
        {
            Email = request.Email
        };

        await _mediator.Send(command);

        return Ok(new { message = "Email z linkiem do resetowania hasła został wysłany" });
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<ActionResult> VerifyEmail([FromBody] VerifyEmailRequestDto request)
    {
        var command = new VerifyEmailCommand
        {
            Token = request.Token,
            Email = request.Email
        };

        await _mediator.Send(command);

        return Ok(new { message = "Email zweryfikowany pomyślnie" });
    }

    [HttpPost("resend-verification")]
    [AllowAnonymous]
    public async Task<ActionResult> ResendVerification([FromBody] ResendVerificationEmailRequestDto request)
    {
        var command = new ResendVerificationEmailCommand
        {
            Email = request.Email
        };

        var result = await _mediator.Send(command);

        if (!result)
        {
            return BadRequest(new { message = "Nie udało się wysłać emaila weryfikacyjnego. Możliwe przyczyny: email został już zweryfikowany, użytkownik nie istnieje, lub limit czasowy (2 minuty) nie upłynął od ostatniego wysłania." });
        }

        return Ok(new { message = "Email weryfikacyjny został wysłany ponownie" });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var user = await _mediator.Send(new GetCurrentUserQuery());

        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Department = user.Department,
            DepartmentId = user.DepartmentId,
            Position = user.Position,
            Role = user.Role.ToString().ToLower(),
            IsEmailVerified = user.IsEmailVerified,
            MustChangePassword = user.MustChangePassword,
            CreatedAt = user.CreatedAt
        };

        return Ok(userDto);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        // Get current user ID from JWT token
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var command = new ChangePasswordCommand
        {
            UserId = userId,
            CurrentPassword = request.CurrentPassword,
            NewPassword = request.NewPassword
        };

        try
        {
            var result = await _mediator.Send(command);

            if (!result)
            {
                return BadRequest(new { message = "Failed to change password" });
            }

            return Ok(new { message = "Password changed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", userId);
            return BadRequest(new { message = ex.Message });
        }
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            // Secure = true only in production (HTTPS), false in development (HTTP)
            Secure = _environment.IsProduction(),
            // SameSite = Lax for development, Strict for production
            SameSite = _environment.IsProduction() ? SameSiteMode.Strict : SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        };

        Response.Cookies.Append("refresh_token", refreshToken, cookieOptions);
    }
}
