using System.Diagnostics;
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
using PortalForge.Api.DTOs.Requests.Requests;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;
using PortalForge.Infrastructure.Persistence;
using Xunit;

namespace PortalForge.Tests.Integration;

/// <summary>
/// Performance tests for API endpoints to ensure they meet performance requirements.
/// </summary>
public class ApiPerformanceTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private WebApplicationFactory<Program> _customFactory = null!;
    private HttpClient _client = null!;
    private Guid _userId;
    private Guid _templateId;
    private readonly List<Guid> _requestIds = new();

    public ApiPerformanceTests(WebApplicationFactory<Program> factory)
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
        _templateId = Guid.NewGuid();

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

        var template = new RequestTemplate
        {
            Id = _templateId,
            Name = "Performance Test Template",
            Description = "Template for performance testing",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedById = _userId
        };

        context.Users.Add(user);
        context.RequestTemplates.Add(template);

        // Create multiple requests for performance testing
        for (int i = 0; i < 100; i++)
        {
            var requestId = Guid.NewGuid();
            _requestIds.Add(requestId);

            var request = new Request
            {
                Id = requestId,
                RequestNumber = $"PERF-{i:D3}",
                RequestTemplateId = _templateId,
                SubmittedById = _userId,
                FormData = $"{{\"field{i}\": \"value{i}\"}}",
                Status = RequestStatus.Approved,
                SubmittedAt = DateTime.UtcNow.AddDays(-i)
            };

            context.Requests.Add(request);
        }

        await context.SaveChangesAsync();
    }

    #region Performance Tests

    [Fact]
    public async Task GET_MyRequests_WithLargeDataset_CompletesWithinTimeLimit()
    {
        // Arrange
        SetAuthHeader(_userId, "User");
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/api/requests/my-requests");

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000); // Should complete within 2 seconds
    }

    [Fact]
    public async Task GET_PersonalAnalytics_CompletesWithinTimeLimit()
    {
        // Arrange
        SetAuthHeader(_userId, "User");
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/api/requests/analytics/personal");

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(3000); // Analytics may take longer
    }

    [Fact]
    public async Task GET_FormBuilder_CompletesWithinTimeLimit()
    {
        // Arrange
        SetAuthHeader(_userId, "User");
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync($"/api/form-builder/templates/{_templateId}/form");

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Form building should be fast
    }

    [Fact]
    public async Task POST_BulkApprove_WithMultipleRequests_CompletesWithinTimeLimit()
    {
        // Arrange
        SetAuthHeader(_userId, "Approver");
        var stepIds = new List<Guid>();

        // Create approval steps for bulk testing
        using (var scope = _customFactory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            for (int i = 0; i < 10; i++)
            {
                var stepId = Guid.NewGuid();
                stepIds.Add(stepId);
                
                var step = new RequestApprovalStep
                {
                    Id = stepId,
                    RequestId = _requestIds[i],
                    ApproverId = _userId,
                    Status = ApprovalStepStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };
                context.RequestApprovalSteps.Add(step);
            }
            
            await context.SaveChangesAsync();
        }

        var dto = new BulkApproveRequestsDto
        {
            RequestStepIds = stepIds,
            Comment = "Bulk approval performance test"
        };

        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.PostAsJsonAsync("/api/requests/bulk-approve", dto);

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // Bulk operations may take longer
    }

    [Fact]
    public async Task Concurrent_RequestCloning_HandlesLoadCorrectly()
    {
        // Arrange
        SetAuthHeader(_userId, "User");
        var tasks = new List<Task<HttpResponseMessage>>();
        var dto = new CloneRequestDto
        {
            ModifiedFormData = "{\"concurrent\": \"test\"}",
            CreateAsTemplate = false
        };

        // Act - Create 10 concurrent clone requests
        for (int i = 0; i < 10; i++)
        {
            var requestId = _requestIds[i];
            tasks.Add(_client.PostAsJsonAsync($"/api/requests/{requestId}/clone", dto));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        foreach (var response in responses)
        {
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
        }

        // At least some requests should succeed
        responses.Count(r => r.StatusCode == HttpStatusCode.Created).Should().BeGreaterThan(0);
    }

    #endregion

    #region Load Tests

    [Fact]
    public async Task Multiple_Concurrent_Analytics_Requests_HandleLoadCorrectly()
    {
        // Arrange
        SetAuthHeader(_userId, "User");
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act - Create multiple concurrent analytics requests
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(_client.GetAsync("/api/requests/analytics/personal"));
            tasks.Add(_client.GetAsync("/api/requests/analytics/processing-time"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        foreach (var response in responses)
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task Form_Validation_Under_Load_PerformsCorrectly()
    {
        // Arrange
        SetAuthHeader(_userId, "User");
        var tasks = new List<Task<HttpResponseMessage>>();
        var dto = new Api.DTOs.Requests.FormBuilder.ValidateFormDataDto
        {
            FormData = "{\"testField\": \"load test value\"}"
        };

        // Act - Create multiple concurrent validation requests
        for (int i = 0; i < 20; i++)
        {
            tasks.Add(_client.PostAsJsonAsync($"/api/form-builder/templates/{_templateId}/validate", dto));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        foreach (var response in responses)
        {
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }
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