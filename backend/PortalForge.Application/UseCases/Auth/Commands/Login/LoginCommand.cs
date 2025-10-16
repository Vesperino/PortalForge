using MediatR;
using PortalForge.Application.Common.Models;

namespace PortalForge.Application.UseCases.Auth.Commands.Login;

public class LoginCommand : IRequest<AuthResult>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
