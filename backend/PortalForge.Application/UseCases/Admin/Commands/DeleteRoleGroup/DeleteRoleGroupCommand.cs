using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Admin.Commands.DeleteRoleGroup;

public class DeleteRoleGroupCommand : IRequest<DeleteRoleGroupResult>, ITransactionalRequest
{
    public Guid RoleGroupId { get; set; }
    public Guid DeletedBy { get; set; }
}

