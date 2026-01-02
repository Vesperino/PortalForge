using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Common.Settings;

namespace PortalForge.Infrastructure.BackgroundJobs;

/// <summary>
/// Background job that runs annually on January 1st to reset vacation allowances.
/// Carries over unused vacation days from the previous year and resets annual counters.
/// Complies with Polish Labor Law regarding vacation day carryover.
/// </summary>
public class UpdateVacationAllowancesJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly VacationSettings _vacationSettings;
    private readonly ILogger<UpdateVacationAllowancesJob> _logger;

    public UpdateVacationAllowancesJob(
        IUnitOfWork unitOfWork,
        IOptions<VacationSettings> vacationSettings,
        ILogger<UpdateVacationAllowancesJob> logger)
    {
        _unitOfWork = unitOfWork;
        _vacationSettings = vacationSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Executes the annual vacation allowance update process.
    /// Runs on January 1st at 00:00 to:
    /// 1. Calculate unused vacation days from previous year
    /// 2. Move unused days to CarriedOverVacationDays (expire Sep 30)
    /// 3. Reset annual vacation counters (uses configured DefaultAnnualDays)
    /// 4. Reset on-demand and circumstantial leave counters
    /// </summary>
    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting annual vacation allowance update");

        try
        {
            var allUsers = await _unitOfWork.UserRepository.GetAllAsync();
            var activeUsers = allUsers.Where(u => u.IsActive).ToList();

            _logger.LogInformation("Processing vacation reset for {UserCount} active users", activeUsers.Count);

            foreach (var user in activeUsers)
            {
                // Calculate unused days from current year
                var unusedDays = user.AnnualVacationDays - user.VacationDaysUsed;

                // Only carry over positive unused days
                if (unusedDays > 0)
                {
                    user.CarriedOverVacationDays = unusedDays;
                    user.CarriedOverExpiryDate = new DateTime(DateTime.UtcNow.Year, 9, 30);

                    _logger.LogInformation(
                        "User {UserId} ({Email}): Carrying over {Days} unused vacation days (expires Sep 30)",
                        user.Id, user.Email, unusedDays);
                }
                else
                {
                    user.CarriedOverVacationDays = 0;
                    user.CarriedOverExpiryDate = null;

                    _logger.LogInformation(
                        "User {UserId} ({Email}): No unused days to carry over",
                        user.Id, user.Email);
                }

                // Reset annual vacation allowance using configured default
                user.AnnualVacationDays = _vacationSettings.DefaultAnnualDays;

                // Reset all counters for the new year
                user.VacationDaysUsed = 0;
                user.OnDemandVacationDaysUsed = 0;
                user.CircumstantialLeaveDaysUsed = 0;

                await _unitOfWork.UserRepository.UpdateAsync(user);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Annual vacation allowance update completed successfully for {UserCount} users",
                activeUsers.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during annual vacation allowance update");
            throw;
        }
    }
}
