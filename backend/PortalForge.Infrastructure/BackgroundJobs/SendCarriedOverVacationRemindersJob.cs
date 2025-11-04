using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Domain.Enums;

namespace PortalForge.Infrastructure.BackgroundJobs;

/// <summary>
/// Background job that runs on September 1st to remind users about expiring carried-over vacation days.
/// Sends notifications to users who still have unused carried-over vacation days
/// that will expire on September 30th if not used.
/// </summary>
public class SendCarriedOverVacationRemindersJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<SendCarriedOverVacationRemindersJob> _logger;

    public SendCarriedOverVacationRemindersJob(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<SendCarriedOverVacationRemindersJob> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Executes the carried-over vacation reminder process.
    /// Runs on September 1st to:
    /// 1. Find all users with carried-over vacation days
    /// 2. Send reminder notifications about the September 30th expiry deadline
    /// 3. Log reminder count for monitoring
    /// </summary>
    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting carried-over vacation reminder notifications");

        try
        {
            var allUsers = await _unitOfWork.UserRepository.GetAllAsync();
            var usersWithCarriedOver = allUsers
                .Where(u => u.IsActive && u.CarriedOverVacationDays > 0)
                .ToList();

            _logger.LogInformation(
                "Found {UserCount} users with carried-over vacation days requiring reminders",
                usersWithCarriedOver.Count);

            var expiryDate = new DateTime(DateTime.UtcNow.Year, 9, 30);
            var daysRemaining = (expiryDate - DateTime.UtcNow).Days;

            foreach (var user in usersWithCarriedOver)
            {
                try
                {
                    await _notificationService.SendVacationExpiryWarningAsync(
                        user.Id,
                        expiryDate,
                        user.CarriedOverVacationDays ?? 0);

                    _logger.LogInformation(
                        "Sent expiry reminder to user {UserId} ({Email}) for {Days} carried-over days",
                        user.Id, user.Email, user.CarriedOverVacationDays ?? 0);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to send expiry reminder to user {UserId} ({Email})",
                        user.Id, user.Email);
                    // Continue with other users even if one fails
                }
            }

            _logger.LogInformation(
                "Carried-over vacation reminder process completed. Reminders sent to {UserCount} users",
                usersWithCarriedOver.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during carried-over vacation reminder process");
            throw;
        }
    }
}
