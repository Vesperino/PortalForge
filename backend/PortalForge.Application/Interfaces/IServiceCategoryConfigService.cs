using PortalForge.Domain.Enums;

namespace PortalForge.Application.Interfaces;

/// <summary>
/// Service for managing service category configurations and routing rules.
/// </summary>
public interface IServiceCategoryConfigService
{
    /// <summary>
    /// Get all available service categories.
    /// </summary>
    /// <returns>List of available service categories</returns>
    Task<List<ServiceCategoryConfig>> GetAvailableServiceCategoriesAsync();

    /// <summary>
    /// Get service category configuration by category name.
    /// </summary>
    /// <param name="categoryName">The service category name</param>
    /// <returns>Service category configuration or null if not found</returns>
    Task<ServiceCategoryConfig?> GetServiceCategoryConfigAsync(string categoryName);

    /// <summary>
    /// Get routing rules for a specific service category.
    /// </summary>
    /// <param name="categoryName">The service category name</param>
    /// <returns>List of routing rules</returns>
    Task<List<ServiceRoutingRule>> GetRoutingRulesAsync(string categoryName);

    /// <summary>
    /// Update service category configuration.
    /// </summary>
    /// <param name="config">The updated configuration</param>
    Task UpdateServiceCategoryConfigAsync(ServiceCategoryConfig config);
}

/// <summary>
/// Configuration for a service category.
/// </summary>
public class ServiceCategoryConfig
{
    public string CategoryName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int EstimatedProcessingHours { get; set; } = 24;
    public List<ServiceRoutingRule> RoutingRules { get; set; } = new();
}

/// <summary>
/// Routing rule for service requests.
/// </summary>
public class ServiceRoutingRule
{
    public string RuleName { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty; // JSON condition for routing
    public List<Guid> TargetUserIds { get; set; } = new();
    public List<string> TargetRoles { get; set; } = new();
    public int Priority { get; set; } = 0; // Higher priority rules are evaluated first
}