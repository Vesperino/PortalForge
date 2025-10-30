using PortalForge.Application.UseCases.Requests.DTOs;

namespace PortalForge.Application.UseCases.Requests.Queries.GetRequestsToApprove;

public class GetRequestsToApproveResult
{
    public List<RequestDto> Requests { get; set; } = new();
}

