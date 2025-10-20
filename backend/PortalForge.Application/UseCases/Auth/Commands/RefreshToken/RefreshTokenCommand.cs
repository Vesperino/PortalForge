using MediatR;
using PortalForge.Application.Common.Models;

namespace PortalForge.Application.UseCases.Auth.Commands.RefreshToken;

public class RefreshTokenCommand : IRequest<AuthResult>
{
    public string RefreshToken { get; set; } = string.Empty;
}
