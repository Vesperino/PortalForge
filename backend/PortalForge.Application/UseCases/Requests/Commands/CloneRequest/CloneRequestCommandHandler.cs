using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.UseCases.Requests.Commands.CloneRequest;

public class CloneRequestCommandHandler : IRequestHandler<CloneRequestCommand, CloneRequestResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CloneRequestCommandHandler> _logger;

    public CloneRequestCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CloneRequestCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CloneRequestResult> Handle(
        CloneRequestCommand command,
        CancellationToken cancellationToken)
    {
        // Get the original request
        var originalRequest = await _unitOfWork.RequestRepository.GetByIdAsync(command.OriginalRequestId);
        if (originalRequest == null)
        {
            throw new Exception("Original request not found");
        }

        // Get the user who is cloning the request
        var clonedByUser = await _unitOfWork.UserRepository.GetByIdAsync(command.ClonedById);
        if (clonedByUser == null)
        {
            throw new Exception("User not found");
        }

        // Validate cloning permissions
        await ValidateClonePermissionsAsync(originalRequest, clonedByUser);

        // Generate new request number
        var year = DateTime.UtcNow.Year;
        var allRequests = await _unitOfWork.RequestRepository.GetAllAsync();
        var requestCount = allRequests.Count() + 1;
        var requestNumber = command.CreateAsTemplate 
            ? $"TMPL-{year}-{requestCount:D4}"
            : $"REQ-{year}-{requestCount:D4}";

        // Create cloned request
        var clonedRequest = new Request
        {
            Id = Guid.NewGuid(),
            RequestNumber = requestNumber,
            RequestTemplateId = originalRequest.RequestTemplateId,
            SubmittedById = command.ClonedById,
            SubmittedAt = DateTime.UtcNow,
            Priority = originalRequest.Priority,
            FormData = command.ModifiedFormData ?? originalRequest.FormData,
            LeaveType = originalRequest.LeaveType,
            Status = command.CreateAsTemplate ? RequestStatus.Draft : RequestStatus.Draft,
            ClonedFromId = originalRequest.Id,
            IsTemplate = command.CreateAsTemplate,
            ServiceCategory = originalRequest.ServiceCategory,
            Tags = originalRequest.Tags
        };

        // Clear sensitive data that shouldn't be cloned
        clonedRequest.Attachments = null; // User should upload new attachments
        clonedRequest.ServiceStatus = null;
        clonedRequest.ServiceCompletedAt = null;
        clonedRequest.ServiceNotes = null;
        clonedRequest.CompletedAt = null;

        // Validate form data if modified
        if (!string.IsNullOrEmpty(command.ModifiedFormData))
        {
            await ValidateModifiedFormDataAsync(command.ModifiedFormData, originalRequest.FormData);
        }

        await _unitOfWork.RequestRepository.CreateAsync(clonedRequest);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Request {OriginalRequestId} cloned successfully as {ClonedRequestId} by user {UserId}",
            command.OriginalRequestId, clonedRequest.Id, command.ClonedById);

        return new CloneRequestResult
        {
            Id = clonedRequest.Id,
            RequestNumber = clonedRequest.RequestNumber,
            Message = command.CreateAsTemplate 
                ? "Request template created successfully" 
                : "Request cloned successfully",
            IsTemplate = command.CreateAsTemplate
        };
    }

    private async Task ValidateClonePermissionsAsync(Request originalRequest, User clonedByUser)
    {
        // Business rules for cloning:
        // 1. User can clone their own requests
        // 2. Admins can clone any request
        // 3. Managers can clone requests from their subordinates
        // 4. Cannot clone requests that are still in progress (unless creating template)

        var isOwner = originalRequest.SubmittedById == clonedByUser.Id;
        var isAdmin = clonedByUser.Role == UserRole.Admin;
        
        if (!isOwner && !isAdmin)
        {
            // Check if user is a manager of the original submitter
            var originalSubmitter = await _unitOfWork.UserRepository.GetByIdAsync(originalRequest.SubmittedById);
            var isManager = originalSubmitter?.SupervisorId == clonedByUser.Id;
            
            if (!isManager)
            {
                throw new UnauthorizedAccessException("You don't have permission to clone this request");
            }
        }

        // Additional validation: cannot clone requests with sensitive data unless authorized
        if (originalRequest.LeaveType == LeaveType.Sick && !isOwner && !isAdmin)
        {
            throw new UnauthorizedAccessException("Cannot clone sick leave requests of other users");
        }

        _logger.LogDebug(
            "Clone permission validated for user {UserId} cloning request {RequestId}",
            clonedByUser.Id, originalRequest.Id);
    }

    private Task ValidateModifiedFormDataAsync(string modifiedFormData, string originalFormData)
    {
        try
        {
            // Validate that modified form data is valid JSON
            var modifiedData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(modifiedFormData);
            var originalData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(originalFormData);

            if (modifiedData == null)
            {
                throw new Exception("Modified form data is not valid JSON");
            }

            // Log the modification for audit purposes
            _logger.LogInformation("Form data modified during cloning. Original fields: {OriginalCount}, Modified fields: {ModifiedCount}",
                originalData?.Count ?? 0, modifiedData.Count);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON in modified form data");
            throw new Exception("Modified form data contains invalid JSON");
        }
        
        return Task.CompletedTask;
    }
}