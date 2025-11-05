namespace PortalForge.Domain.Entities;

public class RequestAnalytics
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    /// <summary>
    /// Total number of requests submitted by the user
    /// </summary>
    public int TotalRequests { get; set; }
    
    /// <summary>
    /// Number of approved requests
    /// </summary>
    public int ApprovedRequests { get; set; }
    
    /// <summary>
    /// Number of rejected requests
    /// </summary>
    public int RejectedRequests { get; set; }
    
    /// <summary>
    /// Number of pending requests
    /// </summary>
    public int PendingRequests { get; set; }
    
    /// <summary>
    /// Average processing time in hours
    /// </summary>
    public double AverageProcessingTime { get; set; }
    
    /// <summary>
    /// When these analytics were last calculated
    /// </summary>
    public DateTime LastCalculated { get; set; }
    
    /// <summary>
    /// Year for which these analytics are calculated
    /// </summary>
    public int Year { get; set; }
    
    /// <summary>
    /// Month for which these analytics are calculated (1-12)
    /// </summary>
    public int Month { get; set; }
}