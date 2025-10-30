using MediatR;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.Requests.DTOs;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.UseCases.Requests.Queries.GetPendingApprovals;

public class GetPendingApprovalsQueryHandler 
    : IRequestHandler<GetPendingApprovalsQuery, GetPendingApprovalsResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPendingApprovalsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetPendingApprovalsResult> Handle(
        GetPendingApprovalsQuery query, 
        CancellationToken cancellationToken)
    {
        // Get all requests where user is an approver with InReview status
        var allRequests = await _unitOfWork.RequestRepository.GetAllAsync();
        
        var pendingRequests = allRequests
            .Where(r => r.ApprovalSteps.Any(step => 
                step.ApproverId == query.UserId && 
                step.Status == ApprovalStepStatus.InReview))
            .OrderByDescending(r => r.SubmittedAt)
            .ToList();

        var requestDtos = pendingRequests.Select(r => new RequestDto
        {
            Id = r.Id,
            RequestNumber = r.RequestNumber,
            RequestTemplateId = r.RequestTemplateId,
            RequestTemplateName = r.RequestTemplate.Name,
            RequestTemplateIcon = r.RequestTemplate.Icon,
            SubmittedById = r.SubmittedById,
            SubmittedByName = $"{r.SubmittedBy.FirstName} {r.SubmittedBy.LastName}",
            SubmittedAt = r.SubmittedAt,
            Priority = r.Priority.ToString(),
            FormData = r.FormData,
            Status = r.Status.ToString(),
            CompletedAt = r.CompletedAt,
            ApprovalSteps = r.ApprovalSteps.Select(step => new RequestApprovalStepDto
            {
                Id = step.Id,
                StepOrder = step.StepOrder,
                ApproverId = step.ApproverId,
                ApproverName = $"{step.Approver.FirstName} {step.Approver.LastName}",
                Status = step.Status.ToString(),
                StartedAt = step.StartedAt,
                FinishedAt = step.FinishedAt,
                Comment = step.Comment,
                RequiresQuiz = step.RequiresQuiz,
                QuizScore = step.QuizScore,
                QuizPassed = step.QuizPassed
            }).ToList()
        }).ToList();

        return new GetPendingApprovalsResult
        {
            Requests = requestDtos
        };
    }
}

