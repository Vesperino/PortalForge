using MediatR;

namespace PortalForge.Application.UseCases.Admin.Queries.GetRoleGroupById;

public class GetRoleGroupByIdQuery : IRequest<GetRoleGroupByIdResult>
{
    public Guid RoleGroupId { get; set; }
}

