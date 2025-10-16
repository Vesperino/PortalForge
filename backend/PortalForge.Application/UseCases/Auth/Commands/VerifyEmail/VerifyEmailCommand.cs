using MediatR;

namespace PortalForge.Application.UseCases.Auth.Commands.VerifyEmail;

public class VerifyEmailCommand : IRequest<Unit>
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
