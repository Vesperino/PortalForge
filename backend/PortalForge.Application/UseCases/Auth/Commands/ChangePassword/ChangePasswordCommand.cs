using MediatR;

namespace PortalForge.Application.UseCases.Auth.Commands.ChangePassword;

public class ChangePasswordCommand : IRequest<bool>
{
    public Guid UserId { get; set; }
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

