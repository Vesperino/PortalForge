using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Admin.Commands.CreateRoleGroup;

public class CreateRoleGroupCommand : IRequest<CreateRoleGroupResult>, ITransactionalRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Guid> PermissionIds { get; set; } = new();
    public Guid CreatedBy { get; set; }
}

