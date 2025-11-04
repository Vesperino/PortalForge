using MediatR;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.Requests.DTOs;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.UseCases.Requests.Queries.GetApprovalsHistory;

public class GetApprovalsHistoryQueryHandler
    : IRequestHandler<GetApprovalsHistoryQuery, GetApprovalsHistoryResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetApprovalsHistoryQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetApprovalsHistoryResult> Handle(
        GetApprovalsHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var requests = await _unitOfWork.RequestRepository.GetByApproverAsync(request.UserId);

        var items = requests
            .SelectMany(r => r.ApprovalSteps
                .Where(s => s.ApproverId == request.UserId &&
                            (s.Status == ApprovalStepStatus.Approved || s.Status == ApprovalStepStatus.Rejected))
                .Select(s => new ApprovalsHistoryItemDto
                {
                    RequestId = r.Id,
                    RequestNumber = r.RequestNumber,
                    TemplateName = r.RequestTemplate?.Name ?? string.Empty,
                    TemplateIcon = r.RequestTemplate?.Icon ?? string.Empty,
                    SubmittedByName = r.SubmittedBy != null ? $"{r.SubmittedBy.FirstName} {r.SubmittedBy.LastName}" : string.Empty,
                    SubmittedAt = r.SubmittedAt,
                    StepId = s.Id,
                    StepOrder = s.StepOrder,
                    Decision = s.Status.ToString(),
                    FinishedAt = s.FinishedAt,
                    Comment = s.Comment
                }))
            .OrderByDescending(i => i.FinishedAt ?? i.SubmittedAt)
            .ToList();

        return new GetApprovalsHistoryResult { Items = items };
    }
}

