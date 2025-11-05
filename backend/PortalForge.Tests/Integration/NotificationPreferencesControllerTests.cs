using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PortalForge.Api.DTOs.Requests.Notifications;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;
using PortalForge.Infrastructure.Persistence;
using Xunit;

namespace PortalForge.Tests.Integration;

/// <summary>
/// Integration tests for NotificationPreferencesController.
/// </summary>
public class NotificationPreferencesControllerTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private WebApplicationFactory<Program> _customFactory = null!;
    private HttpClient _client = null!;
    private Guid _userId;

    public NotificationPreferencesControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        _customFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                var descriptorContext = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptorContext != null)
                {
                    services.Remove(descriptorContext);
                }

                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider();

                var dbName = $"InMemoryTestDb_{Guid.NewGuid()}";
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                    options.UseInternalServiceProvider(serviceProvider);
                    options.EnableSensitiveDataLogging();
                });

                services.AddHttpClient();
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
            });
        });

        _client = _customFactory.CreateClient();
        await SeedTestDataAsync();
    }

    public Task DisposeAsync()
    {
        _client?.Dispose();
        return Task.CompletedTask;
    }

    private async Task SeedTestDataAsync()
    {
        _userId = Guid.NewGuid();

        using var scope = _customFactory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = new User
        {
            Id = _userId,
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var preferences = new NotificationPreferences
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            EmailEnabled = true,
            InAppEnabled = true,
            DigestEnabled = false,
            DigestFrequency = DigestFrequency.Daily,
            DisabledTypes = "[]",
            GroupSimilarNotifications = true,
            MaxGroupSize = 5,
            GroupingTimeWindowMinutes = 60,
            RealTimeEnabled = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);
        context.NotificationPreferences.Add(preferences);
        await context.SaveChangesAsync();
    }

    #region Get Preferences Tests

    [Fact]
    public async Task GET_Preferences_WithValidUser_ReturnsOk()
    {
        // Arrange
        SetAuthHeader(_userId, "User");

        // Act
        var response = await _client.GetAsync("/api/notification-preferences");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GET_Preferences_WithUnauthorizedUser_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/notification-preferences");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Update Preferences Tests

    [Fact]
    public async Task PUT_UpdatePreferences_WithValidData_ReturnsOk()
    {
        // Arrange
        SetAuthHeader(_userId, "User");
        var dto = new UpdateNotificationPreferencesDto
        {
            EmailEnabled = false,
            InAppEnabled = true,
            DigestEnabled = true,
            DigestFrequency = DigestFrequency.Weekly,
            DisabledTypes = new List<NotificationType> { NotificationType.System },
            GroupSimilarNotifications = false,
            MaxGroupSize = 10,
            GroupingTimeWindowMinutes = 120,
            RealTimeEnabled = false
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/notification-preferences", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PUT_UpdatePreferences_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        SetAuthHeader(_userId, "User");
        var dto = new UpdateNotificationPreferencesDto
        {
            MaxGroupSize = -1, // Invalid value
            GroupingTimeWindowMinutes = -60 // Invalid value
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/notification-preferences", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Digest Configuration Tests

    [Fact]
    public async Task GET_DigestConfig_ReturnsOk()
    {
        // Arrange
        SetAuthHeader(_userId, "User");

        // Act
        var response = await _client.GetAsync("/api/notification-preferences/digest-config");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Digest Preview Tests

    [Fact]
    public async Task GET_DigestPreview_WithDailyType_ReturnsOk()
    {
        // Arrange
        SetAuthHeader(_userId, "User");

        // Act
        var response = await _client.GetAsync("/api/notification-preferences/digest-preview?type=Daily");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GET_DigestPreview_WithWeeklyType_ReturnsOk()
    {
        // Arrange
        SetAuthHeader(_userId, "User");

        // Act
        var response = await _client.GetAsync("/api/notification-preferences/digest-preview?type=Weekly");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Notification Type Tests

    [Fact]
    public async Task GET_IsNotificationTypeEnabled_WithValidType_ReturnsOk()
    {
        // Arrange
        SetAuthHeader(_userId, "User");

        // Act
        var response = await _client.GetAsync("/api/notification-preferences/type-enabled/System");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GET_IsNotificationTypeEnabled_WithInvalidType_ReturnsBadRequest()
    {
        // Arrange
        SetAuthHeader(_userId, "User");

        // Act
        var response = await _client.GetAsync("/api/notification-preferences/type-enabled/InvalidType");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Helper Methods

    private void SetAuthHeader(Guid userId, string role)
    {
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Test-User-Id", userId.ToString());
        _client.DefaultRequestHeaders.Add("Test-User-Role", role);
    }

    #endregion
}