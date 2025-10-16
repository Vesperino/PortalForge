namespace PortalForge.Application.UseCases.Auth.Commands.Register;

public class RegisterResult
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
