using MediatR;

namespace PortalForge.Application.UseCases.Requests.Queries.GetRequestsToApprove;

public class GetRequestsToApproveQuery : IRequest<GetRequestsToApproveResult>
{
    public Guid ApproverId { get; set; }
}

