namespace PortalForge.Application.UseCases.Admin.Commands.CreateUser;

public class CreateUserResult
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

