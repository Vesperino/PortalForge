using MediatR;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.Requests.DTOs;

namespace PortalForge.Application.UseCases.Requests.Queries.GetMyRequests;

public class GetMyRequestsQueryHandler 
    : IRequestHandler<GetMyRequestsQuery, GetMyRequestsResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMyRequestsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetMyRequestsResult> Handle(
        GetMyRequestsQuery request, 
        CancellationToken cancellationToken)
    {
        var requests = await _unitOfWork.RequestRepository.GetBySubmitterAsync(request.UserId);

        var requestDtos = requests.Select(r => new RequestDto
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

        return new GetMyRequestsResult
        {
            Requests = requestDtos
        };
    }
}

