using MediatR;
using PortalForge.Application.UseCases.Auth.DTOs;

namespace PortalForge.Application.UseCases.Auth.Queries.GetCurrentUser;

public class GetCurrentUserQuery : IRequest<UserDto>
{
    public Guid? UserId { get; set; }
}
