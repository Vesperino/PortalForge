using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.Services;

public class RequestAnalyticsService : IRequestAnalyticsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RequestAnalyticsService> _logger;

    public RequestAnalyticsService(
        IUnitOfWork unitOfWork,
        ILogger<RequestAnalyticsService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<RequestAnalytics> CalculateUserAnalyticsAsync(Guid userId, int year, int month)
    {
        var userRequests = await _unitOfWork.RequestRepository.GetBySubmitterAsync(userId);
        
        // Filter requests for the specific period
        var periodRequests = userRequests.Where(r => 
            r.SubmittedAt.Year == year && r.SubmittedAt.Month == month).ToList();

        var analytics = new RequestAnalytics
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Year = year,
            Month = month,
            LastCalculated = DateTime.UtcNow,
            TotalRequests = periodRequests.Count,
            ApprovedRequests = periodRequests.Count(r => r.Status == RequestStatus.Approved),
            RejectedRequests = periodRequests.Count(r => r.Status == RequestStatus.Rejected),
            PendingRequests = periodRequests.Count(r => 
                r.Status == RequestStatus.Draft || r.Status == RequestStatus.InReview),
            AverageProcessingTime = CalculateAverageProcessingTime(periodRequests)
        };

        // Save or update analytics
        var existingAnalytics = await _unitOfWork.RequestAnalyticsRepository
            .GetByUserAndPeriodAsync(userId, year, month);

        if (existingAnalytics != null)
        {
            existingAnalytics.TotalRequests = analytics.TotalRequests;
            existingAnalytics.ApprovedRequests = analytics.ApprovedRequests;
            existingAnalytics.RejectedRequests = analytics.RejectedRequests;
            existingAnalytics.PendingRequests = analytics.PendingRequests;
            existingAnalytics.AverageProcessingTime = analytics.AverageProcessingTime;
            existingAnalytics.LastCalculated = analytics.LastCalculated;
            
            await _unitOfWork.RequestAnalyticsRepository.UpdateAsync(existingAnalytics);
            analytics = existingAnalytics;
        }
        else
        {
            await _unitOfWork.RequestAnalyticsRepository.CreateAsync(analytics);
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Analytics calculated for user {UserId} for {Year}-{Month}: {TotalRequests} total requests",
            userId, year, month, analytics.TotalRequests);

        return analytics;
    }

    public async Task CalculateAllUsersAnalyticsAsync(int year, int month)
    {
        var allUsers = await _unitOfWork.UserRepository.GetAllAsync();
        
        foreach (var user in allUsers)
        {
            try
            {
                await CalculateUserAnalyticsAsync(user.Id, year, month);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Failed to calculate analytics for user {UserId} for {Year}-{Month}", 
                    user.Id, year, month);
            }
        }

        _logger.LogInformation(
            "Analytics calculation completed for all users for {Year}-{Month}", 
            year, month);
    }

    public async Task<PersonalAnalytics> GetPersonalAnalyticsAsync(Guid userId, int year)
    {
        var userRequests = await _unitOfWork.RequestRepository.GetBySubmitterAsync(userId);
        var yearRequests = userRequests.Where(r => r.SubmittedAt.Year == year).ToList();

        var analytics = new PersonalAnalytics
        {
            UserId = userId,
            Year = year,
            TotalRequests = yearRequests.Count,
            ApprovedRequests = yearRequests.Count(r => r.Status == RequestStatus.Approved),
            RejectedRequests = yearRequests.Count(r => r.Status == RequestStatus.Rejected),
            PendingRequests = yearRequests.Count(r => 
                r.Status == RequestStatus.Draft || r.Status == RequestStatus.InReview),
            AverageProcessingTime = CalculateAverageProcessingTime(yearRequests)
        };

        analytics.ApprovalRate = analytics.TotalRequests > 0 
            ? (double)analytics.ApprovedRequests / analytics.TotalRequests * 100 
            : 0;

        // Monthly breakdown
        analytics.MonthlyBreakdown = Enumerable.Range(1, 12)
            .Select(month => 
            {
                var monthRequests = yearRequests.Where(r => r.SubmittedAt.Month == month).ToList();
                return new MonthlyStats
                {
                    Month = month,
                    MonthName = new DateTime(year, month, 1).ToString("MMMM"),
                    TotalRequests = monthRequests.Count,
                    ApprovedRequests = monthRequests.Count(r => r.Status == RequestStatus.Approved),
                    RejectedRequests = monthRequests.Count(r => r.Status == RequestStatus.Rejected),
                    PendingRequests = monthRequests.Count(r => 
                        r.Status == RequestStatus.Draft || r.Status == RequestStatus.InReview),
                    AverageProcessingTime = CalculateAverageProcessingTime(monthRequests)
                };
            }).ToList();

        // Request type breakdown
        analytics.RequestTypeBreakdown = yearRequests
            .GroupBy(r => r.RequestTemplate?.Name ?? "Unknown")
            .Select(g => new RequestTypeStats
            {
                RequestType = g.Key,
                Count = g.Count(),
                ApprovalRate = g.Count() > 0 
                    ? (double)g.Count(r => r.Status == RequestStatus.Approved) / g.Count() * 100 
                    : 0,
                AverageProcessingTime = CalculateAverageProcessingTime(g.ToList())
            }).ToList();

        // Processing time breakdown
        analytics.ProcessingTimeBreakdown = CalculateProcessingTimeBreakdown(yearRequests);

        return analytics;
    }

    public async Task<ProcessingTimeAnalytics> GetProcessingTimeAnalyticsAsync(Guid userId, int year, int? month = null)
    {
        var userRequests = await _unitOfWork.RequestRepository.GetBySubmitterAsync(userId);
        var filteredRequests = userRequests.Where(r => r.SubmittedAt.Year == year);

        if (month.HasValue)
        {
            filteredRequests = filteredRequests.Where(r => r.SubmittedAt.Month == month.Value);
        }

        var completedRequests = filteredRequests
            .Where(r => r.CompletedAt.HasValue)
            .ToList();

        var processingTimes = completedRequests
            .Select(r => (r.CompletedAt!.Value - r.SubmittedAt).TotalHours)
            .Where(hours => hours >= 0)
            .OrderBy(hours => hours)
            .ToList();

        var analytics = new ProcessingTimeAnalytics
        {
            UserId = userId,
            Year = year,
            Month = month,
            AverageProcessingTime = processingTimes.Any() ? processingTimes.Average() : 0,
            MedianProcessingTime = processingTimes.Any() 
                ? processingTimes[processingTimes.Count / 2] 
                : 0,
            MinProcessingTime = processingTimes.Any() ? processingTimes.Min() : 0,
            MaxProcessingTime = processingTimes.Any() ? processingTimes.Max() : 0,
            ProcessingTimeBreakdown = CalculateProcessingTimeBreakdown(completedRequests)
        };

        return analytics;
    }

    public async Task RecalculateUserAnalyticsAsync(Guid userId)
    {
        var currentDate = DateTime.UtcNow;
        
        // Recalculate current month
        await CalculateUserAnalyticsAsync(userId, currentDate.Year, currentDate.Month);
        
        // Recalculate previous month if we're early in the current month
        if (currentDate.Day <= 5)
        {
            var previousMonth = currentDate.AddMonths(-1);
            await CalculateUserAnalyticsAsync(userId, previousMonth.Year, previousMonth.Month);
        }

        _logger.LogInformation(
            "Analytics recalculated for user {UserId} due to request status change", 
            userId);
    }

    private double CalculateAverageProcessingTime(List<Request> requests)
    {
        var completedRequests = requests
            .Where(r => r.CompletedAt.HasValue)
            .ToList();

        if (!completedRequests.Any())
            return 0;

        var totalHours = completedRequests
            .Sum(r => (r.CompletedAt!.Value - r.SubmittedAt).TotalHours);

        return totalHours / completedRequests.Count;
    }

    private List<ProcessingTimeStats> CalculateProcessingTimeBreakdown(List<Request> requests)
    {
        var completedRequests = requests
            .Where(r => r.CompletedAt.HasValue)
            .ToList();

        if (!completedRequests.Any())
            return new List<ProcessingTimeStats>();

        var processingTimes = completedRequests
            .Select(r => (r.CompletedAt!.Value - r.SubmittedAt).TotalHours)
            .Where(hours => hours >= 0)
            .ToList();

        var totalCount = processingTimes.Count;

        return new List<ProcessingTimeStats>
        {
            new() 
            { 
                Category = "0-24 hours", 
                Count = processingTimes.Count(h => h <= 24),
                Percentage = totalCount > 0 ? (double)processingTimes.Count(h => h <= 24) / totalCount * 100 : 0
            },
            new() 
            { 
                Category = "1-3 days", 
                Count = processingTimes.Count(h => h > 24 && h <= 72),
                Percentage = totalCount > 0 ? (double)processingTimes.Count(h => h > 24 && h <= 72) / totalCount * 100 : 0
            },
            new() 
            { 
                Category = "3-7 days", 
                Count = processingTimes.Count(h => h > 72 && h <= 168),
                Percentage = totalCount > 0 ? (double)processingTimes.Count(h => h > 72 && h <= 168) / totalCount * 100 : 0
            },
            new() 
            { 
                Category = "1-2 weeks", 
                Count = processingTimes.Count(h => h > 168 && h <= 336),
                Percentage = totalCount > 0 ? (double)processingTimes.Count(h => h > 168 && h <= 336) / totalCount * 100 : 0
            },
            new() 
            { 
                Category = "More than 2 weeks", 
                Count = processingTimes.Count(h => h > 336),
                Percentage = totalCount > 0 ? (double)processingTimes.Count(h => h > 336) / totalCount * 100 : 0
            }
        };
    }
}