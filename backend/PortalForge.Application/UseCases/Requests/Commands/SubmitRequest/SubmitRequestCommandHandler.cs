using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.UseCases.Requests.Commands.SubmitRequest;

public class SubmitRequestCommandHandler
    : IRequestHandler<SubmitRequestCommand, SubmitRequestResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IRequestRoutingService _routingService;
    private readonly IVacationCalculationService _vacationCalculationService;
    private readonly ILogger<SubmitRequestCommandHandler> _logger;

    public SubmitRequestCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        IRequestRoutingService routingService,
        IVacationCalculationService vacationCalculationService,
        ILogger<SubmitRequestCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _routingService = routingService;
        _vacationCalculationService = vacationCalculationService;
        _logger = logger;
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

        // Validate vacation availability if this is a vacation request
        if (template.IsVacationRequest)
        {
            await ValidateVacationRequestAsync(command.FormData, command.SubmittedById);
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

        // Try to persist parsed metadata (leave type, attachments)
        try
        {
            var formDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(command.FormData);
            if (formDict != null)
            {
                // Detect leave type if present in form data
                foreach (var kv in formDict)
                {
                    if (kv.Value.ValueKind == JsonValueKind.String)
                    {
                        var str = kv.Value.GetString();
                        if (!string.IsNullOrWhiteSpace(str))
                        {
                            if (str == "Annual" || str == "OnDemand" || str == "Circumstantial" || str == "Sick")
                            {
                                if (Enum.TryParse<LeaveType>(str, out var lt))
                                {
                                    request.LeaveType = lt;
                                }
                            }
                            else
                            {
                                // Fallback for localized labels
                                var lower = str.ToLowerInvariant();
                                if (lower.Contains("wypocz") || lower == "urlop") request.LeaveType = LeaveType.Annual;
                                else if (lower.Contains("żąd") || lower.Contains("zadanie") || lower.Contains("on demand") || lower.Contains("ondemand")) request.LeaveType = LeaveType.OnDemand;
                                else if (lower.Contains("okolicz")) request.LeaveType = LeaveType.Circumstantial;
                                else if (lower.Contains("zwolnienie") || lower.Contains("l4")) request.LeaveType = LeaveType.Sick;
                            }
                        }
                    }
                }

                // Attachments (e.g., base64 images) if frontend provided under a common key
                if (formDict.TryGetValue("attachments", out var attachmentsEl) && attachmentsEl.ValueKind == JsonValueKind.Array)
                {
                    var list = new List<string>();
                    foreach (var item in attachmentsEl.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.String)
                        {
                            var s = item.GetString();
                            if (!string.IsNullOrWhiteSpace(s)) list.Add(s!);
                        }
                    }
                    if (list.Count > 0)
                    {
                        request.Attachments = JsonSerializer.Serialize(list);
                    }
                }
            }
        }
        catch
        {
            // Non-fatal – keep going if form data parsing fails here
        }

        // Create approval steps based on template
        if (template.RequiresApproval && template.ApprovalStepTemplates.Any())
        {
            var orderedSteps = template.ApprovalStepTemplates.OrderBy(ast => ast.StepOrder).ToList();

            foreach (var stepTemplate in orderedSteps)
            {
                // Resolve approver using routing service
                var approver = await _routingService.ResolveApproverAsync(stepTemplate, submitter);

                if (approver == null)
                {
                    // This should never happen due to upfront validation, but throw exception as safety net
                    _logger.LogError(
                        "No approver found for step {StepOrder} after validation passed. This is a programming error.",
                        stepTemplate.StepOrder);
                    throw new Exception(
                        $"Failed to resolve approver for step {stepTemplate.StepOrder}. Please contact HR.");
                }

                // Create approval step
                var isFirstPendingStep = stepTemplate.StepOrder == 1;

                var approvalStep = new RequestApprovalStep
                {
                    Id = Guid.NewGuid(),
                    RequestId = request.Id,
                    StepOrder = stepTemplate.StepOrder,
                    ApproverId = approver.Id,
                    Status = isFirstPendingStep
                        ? ApprovalStepStatus.InReview
                        : ApprovalStepStatus.Pending,
                    RequiresQuiz = stepTemplate.RequiresQuiz,
                    PassingScore = stepTemplate.PassingScore,
                    RequestApprovalStepTemplateId = stepTemplate.Id,
                    StartedAt = isFirstPendingStep ? DateTime.UtcNow : null
                };
                request.ApprovalSteps.Add(approvalStep);
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
    /// Validates vacation request by parsing form data and checking vacation availability.
    /// Throws exception if user doesn't have sufficient vacation days.
    /// </summary>
    private async Task ValidateVacationRequestAsync(string formDataJson, Guid userId)
    {
        try
        {
            // Parse form data to extract vacation details
            var formData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(formDataJson);
            if (formData == null)
            {
                throw new Exception("Invalid form data");
            }

            // Extract required fields from form data
            // Field IDs are GUIDs, so we need to find them by checking the values
            string? leaveTypeStr = null;
            DateTime? startDate = null;
            DateTime? endDate = null;

            foreach (var field in formData)
            {
                var value = field.Value;

                // Try to identify the field by its value content
                if (value.ValueKind == JsonValueKind.String)
                {
                    var stringValue = value.GetString();

                    // Leave type field (Annual, OnDemand, Circumstantial)
                    if (stringValue == "Annual" || stringValue == "OnDemand" || stringValue == "Circumstantial")
                    {
                        leaveTypeStr = stringValue;
                    }
                    // Date fields (ISO format: yyyy-MM-dd)
                    else if (DateTime.TryParse(stringValue, out var parsedDate))
                    {
                        if (startDate == null)
                        {
                            startDate = parsedDate;
                        }
                        else if (endDate == null)
                        {
                            endDate = parsedDate;
                        }
                    }
                }
            }

            // Validate required fields
            if (string.IsNullOrEmpty(leaveTypeStr))
            {
                throw new Exception("Typ urlopu jest wymagany");
            }

            if (!startDate.HasValue)
            {
                throw new Exception("Data rozpoczęcia jest wymagana");
            }

            if (!endDate.HasValue)
            {
                throw new Exception("Data zakończenia jest wymagana");
            }

            if (endDate.Value < startDate.Value)
            {
                throw new Exception("Data zakończenia nie może być wcześniejsza niż data rozpoczęcia");
            }

            // Parse leave type
            if (!Enum.TryParse<LeaveType>(leaveTypeStr, out var leaveType))
            {
                throw new Exception($"Nieprawidłowy typ urlopu: {leaveTypeStr}");
            }

            // Check vacation availability
            var (canTake, errorMessage) = await _vacationCalculationService.CanTakeVacationAsync(
                userId,
                startDate.Value,
                endDate.Value,
                leaveType);

            if (!canTake)
            {
                _logger.LogWarning(
                    "Vacation validation failed for user {UserId}: {ErrorMessage}",
                    userId, errorMessage);
                throw new Exception(errorMessage ?? "Nie możesz wziąć tego urlopu");
            }

            _logger.LogInformation(
                "Vacation validation passed for user {UserId}: {LeaveType} from {StartDate} to {EndDate}",
                userId, leaveType, startDate.Value.ToString("yyyy-MM-dd"), endDate.Value.ToString("yyyy-MM-dd"));
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error parsing vacation request form data");
            throw new Exception("Błąd parsowania danych formularza");
        }
    }
}

