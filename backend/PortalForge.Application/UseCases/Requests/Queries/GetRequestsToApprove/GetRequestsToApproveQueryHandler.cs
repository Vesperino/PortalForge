using MediatR;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.Requests.DTOs;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.UseCases.Requests.Queries.GetRequestsToApprove;

public class GetRequestsToApproveQueryHandler 
    : IRequestHandler<GetRequestsToApproveQuery, GetRequestsToApproveResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetRequestsToApproveQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetRequestsToApproveResult> Handle(
        GetRequestsToApproveQuery request, 
        CancellationToken cancellationToken)
    {
        var allRequests = await _unitOfWork.RequestRepository.GetByApproverAsync(request.ApproverId);
        
        // Filter to only requests that have a step in InReview status for this approver
        var requestsToApprove = allRequests
            .Where(r => r.ApprovalSteps.Any(step => 
                step.ApproverId == request.ApproverId && 
                step.Status == ApprovalStepStatus.InReview))
            .ToList();

        var requestDtos = requestsToApprove.Select(r => new RequestDto
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
            ApprovalSteps = r.ApprovalSteps.Select(aps => new RequestApprovalStepDto
            {
                Id = aps.Id,
                StepOrder = aps.StepOrder,
                ApproverId = aps.ApproverId,
                ApproverName = aps.Approver?.FullName ?? string.Empty,
                Status = aps.Status.ToString(),
                StartedAt = aps.StartedAt,
                FinishedAt = aps.FinishedAt,
                Comment = aps.Comment,
                RequiresQuiz = aps.RequiresQuiz,
                QuizScore = aps.QuizScore,
                QuizPassed = aps.QuizPassed
            }).ToList()
        }).ToList();

        return new GetRequestsToApproveResult
        {
            Requests = requestDtos
        };
    }
}

