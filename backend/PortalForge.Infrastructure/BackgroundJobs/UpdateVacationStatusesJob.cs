using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Infrastructure.BackgroundJobs;

/// <summary>
/// Background job that runs daily at 00:01 UTC to update vacation statuses.
/// Activates scheduled vacations and completes finished vacations.
/// Also processes approved sick leave (L4) requests.
/// </summary>
public class UpdateVacationStatusesJob
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

    /// <summary>
    /// Updates vacation statuses by activating scheduled vacations and completing active ones.
    /// Also processes approved sick leave requests to create SickLeave records.
    /// Uses scoped service to ensure proper DbContext lifecycle.
    /// </summary>
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting vacation status update and sick leave processing");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var vacationStatusService = scope.ServiceProvider
                .GetRequiredService<IVacationStatusService>();

            // Update vacation statuses (Scheduled -> Active, Active -> Completed)
            await vacationStatusService.UpdateVacationStatusesAsync(cancellationToken);

            // Process approved sick leave requests (create SickLeave records)
            await vacationStatusService.ProcessApprovedSickLeaveRequestsAsync(cancellationToken);

            _logger.LogInformation("Vacation status update and sick leave processing completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during vacation status update and sick leave processing");
            throw;
        }
    }
}
