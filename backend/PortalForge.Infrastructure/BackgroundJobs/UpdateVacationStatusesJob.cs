using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Services;

namespace PortalForge.Infrastructure.BackgroundJobs;

/// <summary>
/// Background job that runs daily at 00:01 UTC to update vacation statuses.
/// Activates scheduled vacations and completes finished vacations.
/// </summary>
public class UpdateVacationStatusesJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UpdateVacationStatusesJob> _logger;

    public UpdateVacationStatusesJob(
        IServiceProvider serviceProvider,
        ILogger<UpdateVacationStatusesJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("UpdateVacationStatusesJob started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;
                var tomorrow = now.Date.AddDays(1).AddMinutes(1); // 00:01 tomorrow UTC
                var delay = tomorrow - now;

                _logger.LogInformation(
                    "Next vacation status update scheduled for {NextRun} (in {Delay})",
                    tomorrow, delay
                );

                await Task.Delay(delay, stoppingToken);

                if (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("UpdateVacationStatusesJob stopping - cancellation requested");
                    break;
                }

                // Execute vacation status update
                await UpdateVacationStatusesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateVacationStatusesJob - will retry tomorrow");
                // Continue running even if error occurs
            }
        }

        _logger.LogInformation("UpdateVacationStatusesJob stopped");
    }

    /// <summary>
    /// Updates vacation statuses by activating scheduled vacations and completing active ones.
    /// Uses scoped service to ensure proper DbContext lifecycle.
    /// </summary>
    private async Task UpdateVacationStatusesAsync()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var vacationService = scope.ServiceProvider
                .GetRequiredService<IVacationScheduleService>();

            _logger.LogInformation("Starting vacation status update");

            await vacationService.UpdateVacationStatusesAsync();

            _logger.LogInformation("Vacation status update completed");
        }
    }
}
