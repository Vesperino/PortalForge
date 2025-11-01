using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Admin.Commands.UpdateUser;

public class UpdateUserCommand : IRequest<UpdateUserResult>, ITransactionalRequest
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public string Position { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = string.Empty;
    public List<Guid> RoleGroupIds { get; set; } = new();
    public bool IsActive { get; set; }
    public Guid UpdatedBy { get; set; }
}

