using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Domain.Enums;

namespace PortalForge.Infrastructure.BackgroundJobs;

/// <summary>
/// Background job that runs daily at 9:00 AM to check for overdue approval steps.
/// Sends SLA reminder notifications to approvers who have pending requests
/// that have been waiting for more than 3 days (configurable threshold).
/// </summary>
public class CheckApprovalDeadlinesJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<CheckApprovalDeadlinesJob> _logger;

    // SLA threshold: requests waiting longer than this will trigger reminders
    private const int SLA_THRESHOLD_DAYS = 3;

    public CheckApprovalDeadlinesJob(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<CheckApprovalDeadlinesJob> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Executes the approval deadline check process.
    /// Runs daily at 9:00 AM to:
    /// 1. Find all approval steps in "InReview" status older than SLA threshold
    /// 2. Send SLA reminder notifications to approvers
    /// 3. Log overdue approvals for monitoring
    /// </summary>
    public async Task ExecuteAsync()
    {
        _logger.LogInformation(
            "Starting approval deadline check (SLA threshold: {ThresholdDays} days)",
            SLA_THRESHOLD_DAYS);

        try
        {
            var thresholdDate = DateTime.UtcNow.AddDays(-SLA_THRESHOLD_DAYS);

            // Get all requests with approval steps in review
            var allRequests = await _unitOfWork.RequestRepository.GetAllAsync();
            var overdueSteps = allRequests
                .SelectMany(r => r.ApprovalSteps
                    .Where(step => step.Status == ApprovalStepStatus.InReview &&
                                   step.StartedAt.HasValue &&
                                   step.StartedAt.Value < thresholdDate)
                    .Select(step => new { Request = r, Step = step }))
                .ToList();

            _logger.LogInformation(
                "Found {OverdueCount} overdue approval steps requiring reminders",
                overdueSteps.Count);

            foreach (var item in overdueSteps)
            {
                var step = item.Step;
                var request = item.Request;
                var daysOverdue = (DateTime.UtcNow - step.StartedAt!.Value).Days;

                try
                {
                    _logger.LogWarning(
                        "Approval step {StepId} for request {RequestId} is overdue by {DaysOverdue} days. Notifying approver {ApproverId}",
                        step.Id, request.Id, daysOverdue, step.ApproverId);

                    await _notificationService.SendSLAReminderAsync(
                        step.ApproverId,
                        request,
                        daysOverdue);

                    _logger.LogInformation(
                        "SLA reminder sent to approver {ApproverId} for request {RequestId}",
                        step.ApproverId, request.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to send SLA reminder for approval step {StepId} to approver {ApproverId}",
                        step.Id, step.ApproverId);
                    // Continue with other overdue steps even if one fails
                }
            }

            _logger.LogInformation(
                "Approval deadline check completed. SLA reminders sent for {OverdueCount} overdue approvals",
                overdueSteps.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during approval deadline check");
            throw;
        }
    }
}
