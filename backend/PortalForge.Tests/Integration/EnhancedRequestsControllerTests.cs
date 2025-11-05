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
/// Integration tests for enhanced RequestsController endpoints.
/// </summary>
public class EnhancedRequestsControllerTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private WebApplicationFactory<Program> _customFactory = null!;
    private HttpClient _client = null!;
    private Guid _userId;
    private Guid _approverId;
    private Guid _requestId;
    private Guid _templateId;

    public EnhancedRequestsControllerTests(WebApplicationFactory<Program> factory)
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
        _approverId = Guid.NewGuid();
        _requestId = Guid.NewGuid();
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

        var approver = new User
        {
            Id = _approverId,
            FirstName = "Approver",
            LastName = "User",
            Email = "approver@test.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var template = new RequestTemplate
        {
            Id = _templateId,
            Name = "Test Template",
            Description = "Test template for integration tests",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedById = _userId
        };

        var request = new Request
        {
            Id = _requestId,
            RequestNumber = "REQ-001",
            RequestTemplateId = _templateId,
            SubmittedById = _userId,
            FormData = "{}",
            Status = RequestStatus.Approved,
            SubmittedAt = DateTime.UtcNow
        };

        context.Users.AddRange(user, approver);
        context.RequestTemplates.Add(template);
        context.Requests.Add(request);
        await context.SaveChangesAsync();
    }

    #region Clone Request Tests

    [Fact]
    public async Task POST_CloneRequest_WithValidData_ReturnsCreated()
    {
        // Arrange
        SetAuthHeader(_userId, "User");
        var dto = new CloneRequestDto
        {
            ModifiedFormData = "{\"field1\": \"modified value\"}",
            CreateAsTemplate = false
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/requests/{_requestId}/clone", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task POST_CloneRequest_WithUnauthorizedUser_ReturnsUnauthorized()
    {
        // Arrange
        var dto = new CloneRequestDto();

        // Act
        var response = await _client.PostAsJsonAsync($"/api/requests/{_requestId}/clone", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Bulk Approve Tests

    [Fact]
    public async Task POST_BulkApproveRequests_WithValidData_ReturnsOk()
    {
        // Arrange
        SetAuthHeader(_approverId, "Approver");
        var stepId = Guid.NewGuid();
        
        // Create approval step
        using (var scope = _customFactory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var step = new RequestApprovalStep
            {
                Id = stepId,
                RequestId = _requestId,
                ApproverId = _approverId,
                Status = ApprovalStepStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            context.RequestApprovalSteps.Add(step);
            await context.SaveChangesAsync();
        }

        var dto = new BulkApproveRequestsDto
        {
            RequestStepIds = new List<Guid> { stepId },
            Comment = "Bulk approval test"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/requests/bulk-approve", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Service Status Tests

    [Fact]
    public async Task PUT_UpdateServiceTaskStatus_WithValidData_ReturnsOk()
    {
        // Arrange
        SetAuthHeader(_approverId, "ServiceManager");
        var dto = new UpdateServiceTaskStatusDto
        {
            Status = ServiceTaskStatus.InProgress,
            Notes = "Service task in progress"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/requests/{_requestId}/service-status", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Analytics Tests

    [Fact]
    public async Task GET_PersonalAnalytics_ReturnsOk()
    {
        // Arrange
        SetAuthHeader(_userId, "User");

        // Act
        var response = await _client.GetAsync("/api/requests/analytics/personal");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GET_ProcessingTimeAnalytics_ReturnsOk()
    {
        // Arrange
        SetAuthHeader(_userId, "User");

        // Act
        var response = await _client.GetAsync("/api/requests/analytics/processing-time?year=2024");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
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