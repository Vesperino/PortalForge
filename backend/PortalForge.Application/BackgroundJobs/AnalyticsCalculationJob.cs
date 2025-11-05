using Microsoft.Extensions.Logging;
using PortalForge.Application.Interfaces;

namespace PortalForge.Application.BackgroundJobs;

/// <summary>
/// Service for calculating request analytics that can be called periodically
/// This can be integrated with a background job scheduler like Hangfire or Quartz
/// </summary>
public class AnalyticsCalculationJob
{
    private readonly IRequestAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsCalculationJob> _logger;

    public AnalyticsCalculationJob(
        IRequestAnalyticsService analyticsService,
        ILogger<AnalyticsCalculationJob> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    /// <summary>
    /// Calculate analytics for the current period
    /// </summary>
    public async Task ExecuteAsync()
    {
        try
        {
            var currentDate = DateTime.UtcNow;
            
            _logger.LogInformation("Starting analytics calculation for {Year}-{Month}", 
                currentDate.Year, currentDate.Month);

            // Calculate analytics for current month
            await _analyticsService.CalculateAllUsersAnalyticsAsync(currentDate.Year, currentDate.Month);

            // If it's early in the month, also recalculate previous month
            if (currentDate.Day <= 5)
            {
                var previousMonth = currentDate.AddMonths(-1);
                _logger.LogInformation("Also calculating analytics for previous month {Year}-{Month}", 
                    previousMonth.Year, previousMonth.Month);
                
                await _analyticsService.CalculateAllUsersAnalyticsAsync(previousMonth.Year, previousMonth.Month);
            }

            _logger.LogInformation("Analytics calculation completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calculating analytics");
            throw;
        }
    }

    /// <summary>
    /// Calculate analytics for a specific period
    /// </summary>
    public async Task ExecuteForPeriodAsync(int year, int month)
    {
        try
        {
            _logger.LogInformation("Starting analytics calculation for {Year}-{Month}", year, month);
            
            await _analyticsService.CalculateAllUsersAnalyticsAsync(year, month);
            
            _logger.LogInformation("Analytics calculation completed successfully for {Year}-{Month}", year, month);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calculating analytics for {Year}-{Month}", year, month);
            throw;
        }
    }
}