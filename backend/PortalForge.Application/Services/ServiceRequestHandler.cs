using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.Services;

/// <summary>
/// Implementation of service request handling with automatic routing and status tracking.
/// </summary>
public class ServiceRequestHandler : IServiceRequestHandler
{
    private readonly IRequestRepository _requestRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;
    private readonly IServiceCategoryConfigService _serviceCategoryConfigService;
    private readonly IRequestRoutingService _requestRoutingService;
    private readonly ILogger<ServiceRequestHandler> _logger;

    public ServiceRequestHandler(
        IRequestRepository requestRepository,
        IUserRepository userRepository,
        INotificationService notificationService,
        IServiceCategoryConfigService serviceCategoryConfigService,
        IRequestRoutingService requestRoutingService,
        ILogger<ServiceRequestHandler> logger)
    {
        _requestRepository = requestRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
        _serviceCategoryConfigService = serviceCategoryConfigService;
        _requestRoutingService = requestRoutingService;
        _logger = logger;
    }

    public async Task<ServiceRequestResult> ProcessServiceRequestAsync(Request request)
    {
        try
        {
            _logger.LogInformation("Processing service request {RequestId} with category {ServiceCategory}", 
                request.Id, request.ServiceCategory);

            if (string.IsNullOrEmpty(request.ServiceCategory))
            {
                return new ServiceRequestResult
                {
                    Success = false,
                    ErrorMessage = "Service category is required for service requests"
                };
            }

            if (!await CanHandleRequestTypeAsync(request.ServiceCategory))
            {
                return new ServiceRequestResult
                {
                    Success = false,
                    ErrorMessage = $"Unknown service category: {request.ServiceCategory}"
                };
            }

            // Update request status to indicate it's been assigned to service team
            request.ServiceStatus = ServiceTaskStatus.Assigned;
            await _requestRepository.UpdateAsync(request);

            // Notify the service team
            await NotifyServiceTeamAsync(request, request.ServiceCategory);

            var config = await _serviceCategoryConfigService.GetServiceCategoryConfigAsync(request.ServiceCategory);
            
            _logger.LogInformation("Successfully processed service request {RequestId} and assigned to {TeamName}", 
                request.Id, config?.DisplayName ?? request.ServiceCategory);

            return new ServiceRequestResult
            {
                Success = true,
                AssignedTeam = config?.DisplayName ?? request.ServiceCategory,
                NotifiedUsers = new List<Guid>(), // In a real implementation, this would contain actual user IDs
                ProcessedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing service request {RequestId}", request.Id);
            return new ServiceRequestResult
            {
                Success = false,
                ErrorMessage = $"Error processing service request: {ex.Message}"
            };
        }
    }

    public async Task<bool> CanHandleRequestTypeAsync(string requestType)
    {
        var config = await _serviceCategoryConfigService.GetServiceCategoryConfigAsync(requestType);
        return config != null && config.IsActive;
    }

    public async Task NotifyServiceTeamAsync(Request request, string serviceCategory)
    {
        try
        {
            var config = await _serviceCategoryConfigService.GetServiceCategoryConfigAsync(serviceCategory);
            if (config == null)
            {
                _logger.LogWarning("Unknown service category {ServiceCategory} for request {RequestId}", 
                    serviceCategory, request.Id);
                return;
            }

            // Get routing rules for this service category
            var routingRules = await _serviceCategoryConfigService.GetRoutingRulesAsync(serviceCategory);

            var message = $"A new service request has been assigned to {config.DisplayName}. " +
                         $"Request #{request.RequestNumber} submitted by {request.SubmittedBy?.FirstName} {request.SubmittedBy?.LastName}.";

            var notificationTitle = $"New {config.DisplayName} Service Request";

            // In a real implementation, you would:
            // 1. Evaluate routing rules to determine target users
            // 2. Query for users who match the routing criteria
            // 3. Send notifications to each target user
            
            _logger.LogInformation("Would notify {TeamName} about service request {RequestId} using {RuleCount} routing rules", 
                config.DisplayName, request.Id, routingRules.Count);

            // Note: In a complete implementation, you would:
            // foreach (var rule in routingRules)
            // {
            //     var targetUsers = await ResolveTargetUsersFromRule(rule, request);
            //     foreach (var userId in targetUsers)
            //     {
            //         await _notificationService.CreateNotificationAsync(
            //             userId,
            //             NotificationType.ServiceRequest,
            //             notificationTitle,
            //             message,
            //             "Request",
            //             request.Id.ToString(),
            //             $"/requests/{request.Id}"
            //         );
            //     }
            // }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying service team {ServiceCategory} for request {RequestId}", 
                serviceCategory, request.Id);
            throw;
        }
    }

    public async Task UpdateServiceTaskStatusAsync(Guid requestId, ServiceTaskStatus status, string notes)
    {
        try
        {
            _logger.LogInformation("Updating service task status for request {RequestId} to {Status}", 
                requestId, status);

            var request = await _requestRepository.GetByIdAsync(requestId);
            if (request == null)
            {
                throw new InvalidOperationException($"Request with ID {requestId} not found");
            }

            if (string.IsNullOrEmpty(request.ServiceCategory))
            {
                throw new InvalidOperationException($"Request {requestId} is not a service request");
            }

            // Update service status and notes
            request.ServiceStatus = status;
            request.ServiceNotes = string.IsNullOrEmpty(request.ServiceNotes) 
                ? notes 
                : $"{request.ServiceNotes}\n{DateTime.UtcNow:yyyy-MM-dd HH:mm}: {notes}";

            // Set completion time if status is completed
            if (status == ServiceTaskStatus.Completed)
            {
                request.ServiceCompletedAt = DateTime.UtcNow;
                
                // Also update the overall request status if it's still in review
                if (request.Status == RequestStatus.InReview)
                {
                    request.Status = RequestStatus.Approved;
                    request.CompletedAt = DateTime.UtcNow;
                }
            }

            await _requestRepository.UpdateAsync(request);

            // Notify the submitter about the status update
            var statusMessage = status switch
            {
                ServiceTaskStatus.InProgress => "Your service request is now being processed.",
                ServiceTaskStatus.Completed => "Your service request has been completed.",
                ServiceTaskStatus.OnHold => "Your service request has been put on hold.",
                ServiceTaskStatus.Cancelled => "Your service request has been cancelled.",
                _ => $"Your service request status has been updated to {status}."
            };

            if (!string.IsNullOrEmpty(notes))
            {
                statusMessage += $" Notes: {notes}";
            }

            await _notificationService.NotifySubmitterAsync(request, statusMessage, NotificationType.RequestCompleted);

            _logger.LogInformation("Successfully updated service task status for request {RequestId}", requestId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating service task status for request {RequestId}", requestId);
            throw;
        }
    }


}