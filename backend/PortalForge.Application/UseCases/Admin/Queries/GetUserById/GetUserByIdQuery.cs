using MediatR;
using PortalForge.Application.UseCases.Admin.Queries.GetUsers;

namespace PortalForge.Application.UseCases.Admin.Queries.GetUserById;

public class GetUserByIdQuery : IRequest<AdminUserDto>
{
    public Guid UserId { get; set; }
}

