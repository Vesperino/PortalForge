using MediatR;

namespace PortalForge.Application.UseCases.Requests.Queries.GetPendingApprovals;

public class GetPendingApprovalsQuery : IRequest<GetPendingApprovalsResult>
{
    public Guid UserId { get; set; }
}

