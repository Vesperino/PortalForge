using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Auth.Commands.Register;

public class RegisterCommand : IRequest<RegisterResult>, ITransactionalRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
}
