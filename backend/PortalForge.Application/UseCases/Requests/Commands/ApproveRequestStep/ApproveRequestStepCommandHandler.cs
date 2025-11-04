using MediatR;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.UseCases.Requests.Commands.ApproveRequestStep;

public class ApproveRequestStepCommandHandler
    : IRequestHandler<ApproveRequestStepCommand, ApproveRequestStepResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IVacationScheduleService _vacationService;

    public ApproveRequestStepCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        IVacationScheduleService vacationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _vacationService = vacationService;
    }

    public async Task<ApproveRequestStepResult> Handle(
        ApproveRequestStepCommand command, 
        CancellationToken cancellationToken)
    {
        var request = await _unitOfWork.RequestRepository.GetByIdAsync(command.RequestId);
        if (request == null)
        {
            return new ApproveRequestStepResult
            {
                Success = false,
                Message = "Request not found"
            };
        }

        var step = request.ApprovalSteps.FirstOrDefault(s => s.Id == command.StepId);
        if (step == null)
        {
            return new ApproveRequestStepResult
            {
                Success = false,
                Message = "Approval step not found"
            };
        }

        if (step.ApproverId != command.ApproverId)
        {
            return new ApproveRequestStepResult
            {
                Success = false,
                Message = "Unauthorized: You are not the approver for this step"
            };
        }

        if (step.Status != ApprovalStepStatus.InReview)
        {
            return new ApproveRequestStepResult
            {
                Success = false,
                Message = "This step is not in review status"
            };
        }

        // Check if quiz is required and not passed
        if (step.RequiresQuiz && step.QuizPassed != true)
        {
            step.Status = ApprovalStepStatus.RequiresSurvey;
            request.Status = RequestStatus.AwaitingSurvey;
            await _unitOfWork.SaveChangesAsync();
            
            return new ApproveRequestStepResult
            {
                Success = false,
                Message = "Quiz must be completed before approval"
            };
        }

        // Approve the step
        step.Status = ApprovalStepStatus.Approved;
        step.FinishedAt = DateTime.UtcNow;
        step.Comment = command.Comment;

        // Check if there are more pending steps
        var nextStep = request.ApprovalSteps
            .Where(s => s.Status == ApprovalStepStatus.Pending)
            .OrderBy(s => s.StepOrder)
            .FirstOrDefault();

        if (nextStep != null)
        {
            // Check if next approver is on vacation
            var originalApproverId = nextStep.ApproverId;
            var substitute = await _vacationService.GetActiveSubstituteAsync(originalApproverId);

            if (substitute != null)
            {
                // Route to substitute instead
                nextStep.ApproverId = substitute.Id;
                nextStep.Comment = $"Routed to substitute for approver on vacation";
            }

            // Move to next step
            nextStep.Status = ApprovalStepStatus.InReview;
            nextStep.StartedAt = DateTime.UtcNow;
            request.Status = RequestStatus.InReview;

            // Notify next approver (original or substitute)
            await _notificationService.NotifyApproverAsync(nextStep.ApproverId, request);
        }
        else
        {
            // All steps approved
            request.Status = RequestStatus.Approved;
            request.CompletedAt = DateTime.UtcNow;

            // Update user's vacation counters for Annual/OnDemand leave
            try
            {
                if (request.RequestTemplate?.IsVacationRequest == true)
                {
                    var dict = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, System.Text.Json.JsonElement>>(request.FormData);
                    string? ltStr = null; System.DateTime? s = null; System.DateTime? e = null;
                    if (dict != null)
                    {
                        foreach (var kv in dict)
                        {
                            var v = kv.Value;
                            if (v.ValueKind == System.Text.Json.JsonValueKind.String)
                            {
                                var str = v.GetString();
                                if (str != null)
                                {
                                    if (ltStr == null && (str == "Annual" || str == "OnDemand" || str == "Circumstantial" || str == "Sick")) ltStr = str;
                                    else if (System.Text.RegularExpressions.Regex.IsMatch(str, "^\\d{4}-\\d{2}-\\d{2}$"))
                                    {
                                        if (!s.HasValue) s = System.DateTime.Parse(str);
                                        else if (!e.HasValue) e = System.DateTime.Parse(str);
                                    }
                                }
                            }
                        }
                    }
                    if (ltStr != null && s.HasValue && e.HasValue && System.Enum.TryParse<PortalForge.Domain.Enums.LeaveType>(ltStr, out var lt))
                    {
                        if (lt == PortalForge.Domain.Enums.LeaveType.Annual || lt == PortalForge.Domain.Enums.LeaveType.OnDemand)
                        {
                            int BusinessDays(System.DateTime start, System.DateTime end)
                            {
                                int days = 0; var cur = start.Date; var last = end.Date;
                                while (cur <= last)
                                {
                                    if (cur.DayOfWeek != System.DayOfWeek.Saturday && cur.DayOfWeek != System.DayOfWeek.Sunday) days++;
                                    cur = cur.AddDays(1);
                                }
                                return days;
                            }
                            var used = BusinessDays(s.Value, e.Value);
                            var user = await _unitOfWork.UserRepository.GetByIdAsync(request.SubmittedById);
                            if (user != null)
                            {
                                if (lt == PortalForge.Domain.Enums.LeaveType.Annual)
                                {
                                    user.VacationDaysUsed = (user.VacationDaysUsed ?? 0) + used;
                                }
                                else // OnDemand
                                {
                                    user.OnDemandVacationDaysUsed = (user.OnDemandVacationDaysUsed ?? 0) + used;
                                    user.VacationDaysUsed = (user.VacationDaysUsed ?? 0) + used;
                                }
                                await _unitOfWork.UserRepository.UpdateAsync(user);
                            }
                        }
                    }
                }
            }
            catch { }

            // Create calendar entry for vacation requests
            try
            {
                if (request.RequestTemplate?.IsVacationRequest == true)
                {
                    await _vacationService.CreateFromApprovedRequestAsync(request);
                }
            }
            catch { }

            // Notify submitter of completion
            await _notificationService.NotifySubmitterAsync(
                request,
                "Wniosek został zatwierdzony i zakończony pomyślnie.",
                NotificationType.RequestCompleted
            );
        }await _unitOfWork.RequestRepository.UpdateAsync(request);
        await _unitOfWork.SaveChangesAsync();

        return new ApproveRequestStepResult
        {
            Success = true,
            Message = request.Status == RequestStatus.Approved
                ? "Request fully approved"
                : "Step approved, moved to next approver"
        };
    }
}

