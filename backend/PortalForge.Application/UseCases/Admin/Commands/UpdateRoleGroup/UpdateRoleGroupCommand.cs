using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Admin.Commands.UpdateRoleGroup;

public class UpdateRoleGroupCommand : IRequest<UpdateRoleGroupResult>, ITransactionalRequest
{
    public Guid RoleGroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Guid> PermissionIds { get; set; } = new();
    public Guid UpdatedBy { get; set; }
}

