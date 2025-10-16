using MediatR;
using PortalForge.Domain.Entities;

namespace PortalForge.Application.UseCases.Auth.Queries.GetCurrentUser;

public class GetCurrentUserQuery : IRequest<User>
{
    public Guid? UserId { get; set; }
}
