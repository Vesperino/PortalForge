using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Infrastructure.BackgroundJobs;

/// <summary>
/// Background job that runs annually on September 30th to expire carried-over vacation days.
/// According to Polish Labor Law, carried-over vacation days from the previous year
/// must be used by September 30th, or they will be forfeited.
/// </summary>
public class ExpireCarriedOverVacationJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ExpireCarriedOverVacationJob> _logger;

    public ExpireCarriedOverVacationJob(
        IUnitOfWork unitOfWork,
        ILogger<ExpireCarriedOverVacationJob> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Executes the carried-over vacation expiration process.
    /// Runs on September 30th at 23:59 to:
    /// 1. Find all users with carried-over vacation days
    /// 2. Reset CarriedOverVacationDays to 0
    /// 3. Clear CarriedOverExpiryDate
    /// 4. Log expiration for audit purposes
    /// </summary>
    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting carried over vacation expiration process");

        try
        {
            var allUsers = await _unitOfWork.UserRepository.GetAllAsync();
            var usersWithCarriedOver = allUsers
                .Where(u => u.CarriedOverVacationDays > 0)
                .ToList();

            _logger.LogInformation(
                "Found {UserCount} users with carried-over vacation days to expire",
                usersWithCarriedOver.Count);

            var totalDaysExpired = 0;

            foreach (var user in usersWithCarriedOver)
            {
                var daysToExpire = user.CarriedOverVacationDays ?? 0;

                _logger.LogWarning(
                    "Expiring {Days} carried-over vacation days for user {UserId} ({Email}). Days not used by deadline.",
                    daysToExpire, user.Id, user.Email);

                // Expire carried-over days
                user.CarriedOverVacationDays = 0;
                user.CarriedOverExpiryDate = null;

                await _unitOfWork.UserRepository.UpdateAsync(user);

                totalDaysExpired += daysToExpire;
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Carried-over vacation expiration completed. Total days expired: {TotalDays} across {UserCount} users",
                totalDaysExpired, usersWithCarriedOver.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during carried-over vacation expiration");
            throw;
        }
    }
}
