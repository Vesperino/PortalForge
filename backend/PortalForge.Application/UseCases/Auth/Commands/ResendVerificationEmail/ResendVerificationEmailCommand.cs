using MediatR;

namespace PortalForge.Application.UseCases.Auth.Commands.ResendVerificationEmail;

public class ResendVerificationEmailCommand : IRequest<bool>
{
    public string Email { get; set; } = string.Empty;
}
