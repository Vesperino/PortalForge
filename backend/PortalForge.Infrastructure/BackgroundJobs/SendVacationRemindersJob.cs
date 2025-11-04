using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Domain.Enums;

namespace PortalForge.Infrastructure.BackgroundJobs;

/// <summary>
/// Background job that sends vacation reminders.
/// Runs daily to check for vacation events and send appropriate notifications.
/// </summary>
public class SendVacationRemindersJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SendVacationRemindersJob> _logger;

    public SendVacationRemindersJob(
        IServiceProvider serviceProvider,
        ILogger<SendVacationRemindersJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Executes the vacation reminder check.
    /// Checks for vacations starting tomorrow, today, ending today, or starting in 7 days.
    /// </summary>
    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting vacation reminder check");

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

                // 1. Vacation starts tomorrow - remind user and substitute
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

                // 2. Vacation starts today - notify substitute about active coverage
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

                // 3. Vacation ends today - welcome user back
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

                // 4. Vacation starts in 7 days - advance notice
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
