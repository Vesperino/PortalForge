using MediatR;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.UseCases.Requests.Commands.SubmitRequest;

public class SubmitRequestCommandHandler
    : IRequestHandler<SubmitRequestCommand, SubmitRequestResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public SubmitRequestCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<SubmitRequestResult> Handle(
        SubmitRequestCommand command, 
        CancellationToken cancellationToken)
    {
        // Get template with approval steps
        var template = await _unitOfWork.RequestTemplateRepository.GetByIdAsync(command.RequestTemplateId);
        if (template == null)
        {
            throw new Exception("Request template not found");
        }

        // Get submitter
        var submitter = await _unitOfWork.UserRepository.GetByIdAsync(command.SubmittedById);
        if (submitter == null)
        {
            throw new Exception("User not found");
        }

        // Generate request number
        var year = DateTime.UtcNow.Year;
        var allRequests = await _unitOfWork.RequestRepository.GetAllAsync();
        var requestCount = allRequests.Count() + 1;
        var requestNumber = $"REQ-{year}-{requestCount:D4}";

        // Create request
        var request = new Request
        {
            Id = Guid.NewGuid(),
            RequestNumber = requestNumber,
            RequestTemplateId = template.Id,
            SubmittedById = submitter.Id,
            SubmittedAt = DateTime.UtcNow,
            Priority = Enum.Parse<RequestPriority>(command.Priority),
            FormData = command.FormData,
            Status = template.RequiresApproval ? RequestStatus.InReview : RequestStatus.Approved
        };

        // Create approval steps based on template
        if (template.RequiresApproval && template.ApprovalStepTemplates.Any())
        {
            var orderedSteps = template.ApprovalStepTemplates.OrderBy(ast => ast.StepOrder).ToList();

            foreach (var stepTemplate in orderedSteps)
            {
                // Resolve approver based on ApproverType
                var approverIds = await ResolveApproversAsync(stepTemplate, submitter);

                foreach (var approverId in approverIds)
                {
                    var approvalStep = new RequestApprovalStep
                    {
                        Id = Guid.NewGuid(),
                        RequestId = request.Id,
                        StepOrder = stepTemplate.StepOrder,
                        ApproverId = approverId,
                        Status = stepTemplate.StepOrder == 1
                            ? ApprovalStepStatus.InReview
                            : ApprovalStepStatus.Pending,
                        RequiresQuiz = stepTemplate.RequiresQuiz,
                        StartedAt = stepTemplate.StepOrder == 1 ? DateTime.UtcNow : null
                    };
                    request.ApprovalSteps.Add(approvalStep);
                }
            }
        }

        await _unitOfWork.RequestRepository.CreateAsync(request);
        await _unitOfWork.SaveChangesAsync();

        // Send notifications to first step approvers
        var firstStepApprovers = request.ApprovalSteps
            .Where(s => s.StepOrder == 1)
            .Select(s => s.ApproverId)
            .Distinct();

        foreach (var approverId in firstStepApprovers)
        {
            await _notificationService.NotifyApproverAsync(approverId, request);
        }

        return new SubmitRequestResult
        {
            Id = request.Id,
            RequestNumber = request.RequestNumber,
            Message = "Request submitted successfully"
        };
    }

    /// <summary>
    /// Resolve approver user IDs based on the approval step template configuration.
    /// </summary>
    private async Task<List<Guid>> ResolveApproversAsync(
        RequestApprovalStepTemplate stepTemplate,
        User submitter)
    {
        var approverIds = new List<Guid>();

        switch (stepTemplate.ApproverType)
        {
            case ApproverType.Role:
                // Hierarchical role-based approval
                if (stepTemplate.ApproverRole == DepartmentRole.Manager)
                {
                    if (submitter.Supervisor != null)
                    {
                        approverIds.Add(submitter.Supervisor.Id);
                    }
                }
                else if (stepTemplate.ApproverRole == DepartmentRole.Director)
                {
                    if (submitter.Supervisor?.Supervisor != null)
                    {
                        approverIds.Add(submitter.Supervisor.Supervisor.Id);
                    }
                }
                break;

            case ApproverType.SpecificUser:
                // Specific user approval
                if (stepTemplate.SpecificUserId.HasValue)
                {
                    approverIds.Add(stepTemplate.SpecificUserId.Value);
                }
                break;

            case ApproverType.UserGroup:
                // Group-based approval - for now, add all users from the group
                // In future, this could be "any one from group" or "all from group"
                if (stepTemplate.ApproverGroupId.HasValue)
                {
                    var group = await _unitOfWork.RoleGroupRepository.GetByIdAsync(
                        stepTemplate.ApproverGroupId.Value
                    );

                    if (group != null)
                    {
                        var userIds = group.UserRoleGroups
                            .Select(urg => urg.UserId)
                            .ToList();
                        approverIds.AddRange(userIds);
                    }
                }
                break;

            case ApproverType.Submitter:
                // Self-approval (e.g., for acknowledgment)
                approverIds.Add(submitter.Id);
                break;
        }

        return approverIds;
    }
}

