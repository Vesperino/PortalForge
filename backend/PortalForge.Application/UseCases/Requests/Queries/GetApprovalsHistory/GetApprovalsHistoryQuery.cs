using MediatR;
using PortalForge.Application.UseCases.Requests.DTOs;

namespace PortalForge.Application.UseCases.Requests.Queries.GetApprovalsHistory;

public class GetApprovalsHistoryQuery : IRequest<GetApprovalsHistoryResult>
{
    public Guid UserId { get; set; }
}

public class GetApprovalsHistoryResult
{
    public List<ApprovalsHistoryItemDto> Items { get; set; } = new();
}

