using Microsoft.Extensions.Logging;
using PortalForge.Application.Interfaces;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.Services;

/// <summary>
/// Implementation of service category configuration management.
/// </summary>
public class ServiceCategoryConfigService : IServiceCategoryConfigService
{
    private readonly ILogger<ServiceCategoryConfigService> _logger;

    // In a real implementation, this would come from database or configuration
    private readonly List<ServiceCategoryConfig> _serviceCategories = new()
    {
        new ServiceCategoryConfig
        {
            CategoryName = "IT",
            DisplayName = "Information Technology",
            Description = "IT support, software requests, hardware issues",
            Icon = "computer",
            IsActive = true,
            EstimatedProcessingHours = 24,
            RoutingRules = new List<ServiceRoutingRule>
            {
                new ServiceRoutingRule
                {
                    RuleName = "Hardware Issues",
                    Condition = "{ \"formData.requestType\": \"hardware\" }",
                    TargetRoles = new List<string> { "ITSupport", "SystemAdmin" },
                    Priority = 1
                },
                new ServiceRoutingRule
                {
                    RuleName = "Software Requests",
                    Condition = "{ \"formData.requestType\": \"software\" }",
                    TargetRoles = new List<string> { "ITSupport" },
                    Priority = 2
                }
            }
        },
        new ServiceCategoryConfig
        {
            CategoryName = "HR",
            DisplayName = "Human Resources",
            Description = "HR policies, employee relations, benefits",
            Icon = "people",
            IsActive = true,
            EstimatedProcessingHours = 48,
            RoutingRules = new List<ServiceRoutingRule>
            {
                new ServiceRoutingRule
                {
                    RuleName = "Benefits Questions",
                    Condition = "{ \"formData.category\": \"benefits\" }",
                    TargetRoles = new List<string> { "HRSpecialist", "BenefitsAdmin" },
                    Priority = 1
                }
            }
        },
        new ServiceCategoryConfig
        {
            CategoryName = "Facilities",
            DisplayName = "Facilities Management",
            Description = "Office maintenance, space requests, equipment",
            Icon = "building",
            IsActive = true,
            EstimatedProcessingHours = 72,
            RoutingRules = new List<ServiceRoutingRule>
            {
                new ServiceRoutingRule
                {
                    RuleName = "Maintenance Requests",
                    Condition = "{ \"formData.type\": \"maintenance\" }",
                    TargetRoles = new List<string> { "FacilitiesManager", "MaintenanceStaff" },
                    Priority = 1
                }
            }
        },
        new ServiceCategoryConfig
        {
            CategoryName = "Finance",
            DisplayName = "Finance",
            Description = "Budget requests, expense reports, financial queries",
            Icon = "dollar-sign",
            IsActive = true,
            EstimatedProcessingHours = 48,
            RoutingRules = new List<ServiceRoutingRule>
            {
                new ServiceRoutingRule
                {
                    RuleName = "Budget Requests",
                    Condition = "{ \"formData.type\": \"budget\" }",
                    TargetRoles = new List<string> { "FinanceManager", "Accountant" },
                    Priority = 1
                }
            }
        },
        new ServiceCategoryConfig
        {
            CategoryName = "Legal",
            DisplayName = "Legal",
            Description = "Contract reviews, legal advice, compliance",
            Icon = "scale",
            IsActive = true,
            EstimatedProcessingHours = 120,
            RoutingRules = new List<ServiceRoutingRule>
            {
                new ServiceRoutingRule
                {
                    RuleName = "Contract Reviews",
                    Condition = "{ \"formData.type\": \"contract\" }",
                    TargetRoles = new List<string> { "LegalCounsel", "ComplianceOfficer" },
                    Priority = 1
                }
            }
        },
        new ServiceCategoryConfig
        {
            CategoryName = "Procurement",
            DisplayName = "Procurement",
            Description = "Purchase requests, vendor management, sourcing",
            Icon = "shopping-cart",
            IsActive = true,
            EstimatedProcessingHours = 96,
            RoutingRules = new List<ServiceRoutingRule>
            {
                new ServiceRoutingRule
                {
                    RuleName = "Purchase Requests",
                    Condition = "{ \"formData.type\": \"purchase\" }",
                    TargetRoles = new List<string> { "ProcurementManager", "PurchasingAgent" },
                    Priority = 1
                }
            }
        }
    };

    public ServiceCategoryConfigService(ILogger<ServiceCategoryConfigService> logger)
    {
        _logger = logger;
    }

    public async Task<List<ServiceCategoryConfig>> GetAvailableServiceCategoriesAsync()
    {
        await Task.CompletedTask; // Async for future database implementation
        return _serviceCategories.Where(c => c.IsActive).ToList();
    }

    public async Task<ServiceCategoryConfig?> GetServiceCategoryConfigAsync(string categoryName)
    {
        await Task.CompletedTask; // Async for future database implementation
        return _serviceCategories.FirstOrDefault(c => 
            c.CategoryName.Equals(categoryName, StringComparison.OrdinalIgnoreCase) && c.IsActive);
    }

    public async Task<List<ServiceRoutingRule>> GetRoutingRulesAsync(string categoryName)
    {
        var config = await GetServiceCategoryConfigAsync(categoryName);
        return config?.RoutingRules.OrderByDescending(r => r.Priority).ToList() ?? new List<ServiceRoutingRule>();
    }

    public async Task UpdateServiceCategoryConfigAsync(ServiceCategoryConfig config)
    {
        try
        {
            _logger.LogInformation("Updating service category configuration for {CategoryName}", config.CategoryName);
            
            var existingIndex = _serviceCategories.FindIndex(c => 
                c.CategoryName.Equals(config.CategoryName, StringComparison.OrdinalIgnoreCase));
            
            if (existingIndex >= 0)
            {
                _serviceCategories[existingIndex] = config;
            }
            else
            {
                _serviceCategories.Add(config);
            }

            await Task.CompletedTask; // In a real implementation, this would save to database
            
            _logger.LogInformation("Successfully updated service category configuration for {CategoryName}", config.CategoryName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating service category configuration for {CategoryName}", config.CategoryName);
            throw;
        }
    }
}