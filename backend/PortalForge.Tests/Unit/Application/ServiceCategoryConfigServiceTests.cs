using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using PortalForge.Application.Interfaces;
using PortalForge.Application.Services;

namespace PortalForge.Tests.Unit.Application;

public class ServiceCategoryConfigServiceTests
{
    private readonly Mock<ILogger<ServiceCategoryConfigService>> _mockLogger;
    private readonly ServiceCategoryConfigService _service;

    public ServiceCategoryConfigServiceTests()
    {
        _mockLogger = new Mock<ILogger<ServiceCategoryConfigService>>();
        _service = new ServiceCategoryConfigService(_mockLogger.Object);
    }

    [Fact]
    public async Task GetAvailableServiceCategoriesAsync_ReturnsActiveCategories()
    {
        // Act
        var result = await _service.GetAvailableServiceCategoriesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(c => c.IsActive);
        
        // Verify expected categories are present
        result.Should().Contain(c => c.CategoryName == "IT");
        result.Should().Contain(c => c.CategoryName == "HR");
        result.Should().Contain(c => c.CategoryName == "Facilities");
        result.Should().Contain(c => c.CategoryName == "Finance");
        result.Should().Contain(c => c.CategoryName == "Legal");
        result.Should().Contain(c => c.CategoryName == "Procurement");
    }

    [Fact]
    public async Task GetServiceCategoryConfigAsync_ValidCategory_ReturnsConfig()
    {
        // Arrange
        var categoryName = "IT";

        // Act
        var result = await _service.GetServiceCategoryConfigAsync(categoryName);

        // Assert
        result.Should().NotBeNull();
        result!.CategoryName.Should().Be("IT");
        result.DisplayName.Should().Be("Information Technology");
        result.IsActive.Should().BeTrue();
        result.RoutingRules.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetServiceCategoryConfigAsync_CaseInsensitive_ReturnsConfig()
    {
        // Arrange
        var categoryName = "hr"; // lowercase

        // Act
        var result = await _service.GetServiceCategoryConfigAsync(categoryName);

        // Assert
        result.Should().NotBeNull();
        result!.CategoryName.Should().Be("HR");
        result.DisplayName.Should().Be("Human Resources");
    }

    [Fact]
    public async Task GetServiceCategoryConfigAsync_NonExistentCategory_ReturnsNull()
    {
        // Arrange
        var categoryName = "NonExistent";

        // Act
        var result = await _service.GetServiceCategoryConfigAsync(categoryName);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRoutingRulesAsync_ValidCategory_ReturnsRulesOrderedByPriority()
    {
        // Arrange
        var categoryName = "IT";

        // Act
        var result = await _service.GetRoutingRulesAsync(categoryName);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        
        // Verify rules are ordered by priority (descending)
        for (int i = 0; i < result.Count - 1; i++)
        {
            result[i].Priority.Should().BeGreaterThanOrEqualTo(result[i + 1].Priority);
        }
    }

    [Fact]
    public async Task GetRoutingRulesAsync_NonExistentCategory_ReturnsEmptyList()
    {
        // Arrange
        var categoryName = "NonExistent";

        // Act
        var result = await _service.GetRoutingRulesAsync(categoryName);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateServiceCategoryConfigAsync_ExistingCategory_UpdatesConfig()
    {
        // Arrange
        var updatedConfig = new ServiceCategoryConfig
        {
            CategoryName = "IT",
            DisplayName = "Updated IT Department",
            Description = "Updated description",
            IsActive = true,
            EstimatedProcessingHours = 48
        };

        // Act
        await _service.UpdateServiceCategoryConfigAsync(updatedConfig);

        // Verify the update by retrieving the config
        var result = await _service.GetServiceCategoryConfigAsync("IT");

        // Assert
        result.Should().NotBeNull();
        result!.DisplayName.Should().Be("Updated IT Department");
        result.Description.Should().Be("Updated description");
        result.EstimatedProcessingHours.Should().Be(48);
    }

    [Fact]
    public async Task UpdateServiceCategoryConfigAsync_NewCategory_AddsConfig()
    {
        // Arrange
        var newConfig = new ServiceCategoryConfig
        {
            CategoryName = "NewCategory",
            DisplayName = "New Department",
            Description = "New department description",
            IsActive = true,
            EstimatedProcessingHours = 24
        };

        // Act
        await _service.UpdateServiceCategoryConfigAsync(newConfig);

        // Verify the addition by retrieving the config
        var result = await _service.GetServiceCategoryConfigAsync("NewCategory");

        // Assert
        result.Should().NotBeNull();
        result!.CategoryName.Should().Be("NewCategory");
        result.DisplayName.Should().Be("New Department");
        result.Description.Should().Be("New department description");
    }

    [Theory]
    [InlineData("IT", "Information Technology")]
    [InlineData("HR", "Human Resources")]
    [InlineData("Facilities", "Facilities Management")]
    [InlineData("Finance", "Finance")]
    [InlineData("Legal", "Legal")]
    [InlineData("Procurement", "Procurement")]
    public async Task GetServiceCategoryConfigAsync_AllPredefinedCategories_ReturnCorrectDisplayNames(
        string categoryName, string expectedDisplayName)
    {
        // Act
        var result = await _service.GetServiceCategoryConfigAsync(categoryName);

        // Assert
        result.Should().NotBeNull();
        result!.DisplayName.Should().Be(expectedDisplayName);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetServiceCategoryConfigAsync_AllCategories_HaveRoutingRules()
    {
        // Arrange
        var categories = new[] { "IT", "HR", "Facilities", "Finance", "Legal", "Procurement" };

        // Act & Assert
        foreach (var category in categories)
        {
            var config = await _service.GetServiceCategoryConfigAsync(category);
            config.Should().NotBeNull($"Category {category} should exist");
            config!.RoutingRules.Should().NotBeEmpty($"Category {category} should have routing rules");
        }
    }
}