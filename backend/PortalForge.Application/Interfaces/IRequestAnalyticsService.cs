using PortalForge.Domain.Entities;

namespace PortalForge.Application.Interfaces;

/// <summary>
/// Service for calculating and managing request analytics
/// </summary>
public interface IRequestAnalyticsService
{
    /// <summary>
    /// Calculate analytics for a specific user and period
    /// </summary>
    Task<RequestAnalytics> CalculateUserAnalyticsAsync(Guid userId, int year, int month);
    
    /// <summary>
    /// Calculate analytics for all users for a specific period
    /// </summary>
    Task CalculateAllUsersAnalyticsAsync(int year, int month);
    
    /// <summary>
    /// Get personal analytics for a user
    /// </summary>
    Task<PersonalAnalytics> GetPersonalAnalyticsAsync(Guid userId, int year);
    
    /// <summary>
    /// Get processing time analytics for requests
    /// </summary>
    Task<ProcessingTimeAnalytics> GetProcessingTimeAnalyticsAsync(Guid userId, int year, int? month = null);
    
    /// <summary>
    /// Recalculate analytics for a user when their request status changes
    /// </summary>
    Task RecalculateUserAnalyticsAsync(Guid userId);
}

public class PersonalAnalytics
{
    public Guid UserId { get; set; }
    public int Year { get; set; }
    public int TotalRequests { get; set; }
    public int ApprovedRequests { get; set; }
    public int RejectedRequests { get; set; }
    public int PendingRequests { get; set; }
    public double ApprovalRate { get; set; }
    public double AverageProcessingTime { get; set; }
    public List<MonthlyStats> MonthlyBreakdown { get; set; } = new();
    public List<RequestTypeStats> RequestTypeBreakdown { get; set; } = new();
    public List<ProcessingTimeStats> ProcessingTimeBreakdown { get; set; } = new();
}

public class MonthlyStats
{
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int TotalRequests { get; set; }
    public int ApprovedRequests { get; set; }
    public int RejectedRequests { get; set; }
    public int PendingRequests { get; set; }
    public double AverageProcessingTime { get; set; }
}

public class RequestTypeStats
{
    public string RequestType { get; set; } = string.Empty;
    public int Count { get; set; }
    public double ApprovalRate { get; set; }
    public double AverageProcessingTime { get; set; }
}

public class ProcessingTimeAnalytics
{
    public Guid UserId { get; set; }
    public int Year { get; set; }
    public int? Month { get; set; }
    public double AverageProcessingTime { get; set; }
    public double MedianProcessingTime { get; set; }
    public double MinProcessingTime { get; set; }
    public double MaxProcessingTime { get; set; }
    public List<ProcessingTimeStats> ProcessingTimeBreakdown { get; set; } = new();
}

public class ProcessingTimeStats
{
    public string Category { get; set; } = string.Empty; // e.g., "0-24 hours", "1-3 days", etc.
    public int Count { get; set; }
    public double Percentage { get; set; }
}