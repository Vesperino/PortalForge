using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Admin.Commands.DeleteUser;

public class DeleteUserCommand : IRequest<DeleteUserResult>, ITransactionalRequest
{
    public Guid UserId { get; set; }
    public Guid DeletedBy { get; set; }
}

