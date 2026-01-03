using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.Requests.DTOs;

namespace PortalForge.Application.UseCases.Requests.Queries.GetAllRequests;

/// <summary>
/// Handler for GetAllRequestsQuery.
/// Returns all requests for Admin/HR users with IDOR protection via IRequireAuthorization.
/// </summary>
public class GetAllRequestsQueryHandler : IRequestHandler<GetAllRequestsQuery, GetAllRequestsResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAllRequestsQueryHandler> _logger;

    public GetAllRequestsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAllRequestsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<GetAllRequestsResult> Handle(
        GetAllRequestsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Admin/HR user {UserId} fetching all requests",
            request.CurrentUserId);

        var requests = await _unitOfWork.RequestRepository.GetAllAsync(cancellationToken);
        var requestList = requests.ToList();

        var requestDtos = requestList.Select(r => new RequestDto
        {
            Id = r.Id,
            RequestNumber = r.RequestNumber,
            RequestTemplateId = r.RequestTemplateId,
            RequestTemplateName = r.RequestTemplate?.Name ?? string.Empty,
            RequestTemplateIcon = r.RequestTemplate?.Icon ?? string.Empty,
            SubmittedById = r.SubmittedById,
            SubmittedByName = r.SubmittedBy?.FullName ?? string.Empty,
            SubmittedAt = r.SubmittedAt,
            Priority = r.Priority.ToString(),
            FormData = r.FormData,
            Status = r.Status.ToString(),
            CompletedAt = r.CompletedAt,
            ApprovalSteps = new List<RequestApprovalStepDto>()
        }).ToList();

        _logger.LogInformation(
            "Returning {Count} requests for Admin/HR user",
            requestDtos.Count);

        return new GetAllRequestsResult
        {
            Requests = requestDtos,
            TotalCount = requestDtos.Count
        };
    }
}
