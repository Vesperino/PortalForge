using MediatR;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.UseCases.Requests.Commands.SubmitRequest;

public class SubmitRequestCommandHandler 
    : IRequestHandler<SubmitRequestCommand, SubmitRequestResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public SubmitRequestCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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
                // Find approver based on role
                User? approver = null;
                
                if (stepTemplate.ApproverRole == DepartmentRole.Manager)
                {
                    // Find direct supervisor
                    approver = submitter.Supervisor;
                }
                else if (stepTemplate.ApproverRole == DepartmentRole.Director)
                {
                    // Find supervisor's supervisor
                    approver = submitter.Supervisor?.Supervisor;
                }

                if (approver != null)
                {
                    var approvalStep = new RequestApprovalStep
                    {
                        Id = Guid.NewGuid(),
                        RequestId = request.Id,
                        StepOrder = stepTemplate.StepOrder,
                        ApproverId = approver.Id,
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

        return new SubmitRequestResult
        {
            Id = request.Id,
            RequestNumber = request.RequestNumber,
            Message = "Request submitted successfully"
        };
    }
}

