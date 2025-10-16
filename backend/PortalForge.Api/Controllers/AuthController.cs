using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalForge.Application.Common.Models;
using PortalForge.Application.UseCases.Auth.Commands.Login;
using PortalForge.Application.UseCases.Auth.Commands.Logout;
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

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
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

        var response = new AuthResponseDto
        {
            User = new UserDto
            {
                Id = result.UserId ?? Guid.Empty,
                Email = result.Email ?? string.Empty,
                FirstName = string.Empty,
                LastName = string.Empty,
                IsEmailVerified = false,
                CreatedAt = DateTime.UtcNow
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
            IsEmailVerified = user.IsEmailVerified,
            CreatedAt = user.CreatedAt
        };

        return Ok(userDto);
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        };

        Response.Cookies.Append("refresh_token", refreshToken, cookieOptions);
    }
}
