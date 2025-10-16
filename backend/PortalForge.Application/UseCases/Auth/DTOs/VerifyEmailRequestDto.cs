namespace PortalForge.Application.UseCases.Auth.DTOs;

public class VerifyEmailRequestDto
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
