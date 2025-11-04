using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Domain.Enums;

namespace PortalForge.Infrastructure.Services;

/// <summary>
/// Background service that sends vacation reminders periodically
/// </summary>
public class VacationReminderBackgroundService : BackgroundService
{
    private readonly ILogger<VacationReminderBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(6); // Check every 6 hours

    public VacationReminderBackgroundService(
        ILogger<VacationReminderBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Vacation Reminder Background Service is starting");

        // Wait for initial delay to avoid startup overhead
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Running vacation reminder check at {Time}", DateTime.UtcNow);
                await CheckAndSendVacationRemindersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking vacation reminders");
            }

            // Wait for the next check interval
            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Vacation Reminder Background Service is stopping");
    }

    private async Task CheckAndSendVacationRemindersAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var inSevenDays = today.AddDays(7);

        try
        {
            // Get all active vacation schedules
            var activeSchedules = await unitOfWork.VacationScheduleRepository.GetAllAsync();

            foreach (var schedule in activeSchedules)
            {
                // Skip if schedule doesn't have required data
                if (schedule.User == null)
                {
                    _logger.LogWarning("Vacation schedule {ScheduleId} has no user loaded", schedule.Id);
                    continue;
                }

                // Check if vacation starts tomorrow - remind user and substitute
                if (schedule.StartDate.Date == tomorrow)
                {
                    _logger.LogInformation(
                        "Sending vacation start reminder for user {UserId} (vacation {ScheduleId})",
                        schedule.UserId,
                        schedule.Id);

                    // Notify the user
                    await notificationService.CreateNotificationAsync(
                        userId: schedule.UserId,
                        type: NotificationType.System,
                        title: "Przypomnienie o urlopie",
                        message: $"Twój urlop rozpocznie się jutro ({schedule.StartDate:yyyy-MM-dd}). Upewnij się, że wszystko jest przygotowane.",
                        relatedEntityType: "VacationSchedule",
                        relatedEntityId: schedule.Id.ToString(),
                        actionUrl: "/dashboard/vacations"
                    );

                    // Notify substitute if assigned
                    if (schedule.SubstituteUserId.HasValue)
                    {
                        await notificationService.CreateNotificationAsync(
                            userId: schedule.SubstituteUserId.Value,
                            type: NotificationType.System,
                            title: "Przypomnienie o zastępstwie",
                            message: $"Od jutra ({schedule.StartDate:yyyy-MM-dd}) będziesz zastępcą dla {schedule.User.FirstName} {schedule.User.LastName}.",
                            relatedEntityType: "VacationSchedule",
                            relatedEntityId: schedule.Id.ToString(),
                            actionUrl: "/dashboard/substitutions"
                        );
                    }
                }

                // Check if vacation starts today - notify substitute about active coverage
                if (schedule.StartDate.Date == today && schedule.SubstituteUserId.HasValue)
                {
                    _logger.LogInformation(
                        "Notifying substitute {SubstituteId} about vacation coverage starting (vacation {ScheduleId})",
                        schedule.SubstituteUserId,
                        schedule.Id);

                    await notificationService.NotifyVacationStartedAsync(
                        schedule.SubstituteUserId.Value,
                        schedule);
                }

                // Check if vacation ends today - welcome user back
                if (schedule.EndDate.Date == today)
                {
                    _logger.LogInformation(
                        "Sending vacation end notification for user {UserId} (vacation {ScheduleId})",
                        schedule.UserId,
                        schedule.Id);

                    await notificationService.NotifyVacationEndedAsync(
                        schedule.UserId,
                        schedule);
                }

                // Check if vacation starts in 7 days - advance notice
                if (schedule.StartDate.Date == inSevenDays)
                {
                    _logger.LogInformation(
                        "Sending advance vacation notice for user {UserId} (vacation {ScheduleId})",
                        schedule.UserId,
                        schedule.Id);

                    await notificationService.CreateNotificationAsync(
                        userId: schedule.UserId,
                        type: NotificationType.System,
                        title: "Zbliżający się urlop",
                        message: $"Za tydzień rozpoczyna się Twój urlop ({schedule.StartDate:yyyy-MM-dd} - {schedule.EndDate:yyyy-MM-dd}). Pamiętaj o przekazaniu spraw.",
                        relatedEntityType: "VacationSchedule",
                        relatedEntityId: schedule.Id.ToString(),
                        actionUrl: "/dashboard/vacations"
                    );
                }
            }

            _logger.LogInformation("Vacation reminder check completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing vacation reminders");
            throw;
        }
    }
}
