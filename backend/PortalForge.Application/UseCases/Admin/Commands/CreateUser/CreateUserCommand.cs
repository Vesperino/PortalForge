using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Admin.Commands.CreateUser;

public class CreateUserCommand : IRequest<CreateUserResult>, ITransactionalRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public string Position { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string Role { get; set; } = "Employee";
    public List<Guid> RoleGroupIds { get; set; } = new();
    public bool MustChangePassword { get; set; } = true;
    public Guid CreatedBy { get; set; }
}

