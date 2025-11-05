using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PortalForge.Application.DTOs;
using PortalForge.Domain.Entities;
using PortalForge.Infrastructure.Persistence;
using Xunit;

namespace PortalForge.Tests.Integration;

/// <summary>
/// Integration tests for DepartmentsController.
/// Tests real HTTP requests against an in-memory database.
/// </summary>
public class DepartmentsControllerTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private WebApplicationFactory<Program> _customFactory = null!;
    private HttpClient _client = null!;
    private Guid _adminUserId;
    private Guid _regularUserId;

    public DepartmentsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        // Create a new factory with in-memory database
        _customFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Add test configuration for Supabase JWT secret
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Supabase:JwtSecret"] = "test-jwt-secret-for-integration-tests-only-not-for-production-use"
                });
            });

            builder.ConfigureTestServices(services =>
            {
                // Remove the production DbContext registration
                var descriptorContext = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptorContext != null)
                {
                    services.Remove(descriptorContext);
                }

                // Create a separate service provider for the InMemory database to avoid conflicts
                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider();

                // Add in-memory database with separate service provider
                var dbName = $"InMemoryTestDb_{Guid.NewGuid()}";
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                    options.UseInternalServiceProvider(serviceProvider);
                    options.EnableSensitiveDataLogging();
                });

                // Add HttpClient for GeocodingService
                services.AddHttpClient();

                // Add test authentication
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
            });
        });

        _client = _customFactory.CreateClient();

        // Seed test data
        await SeedTestDataAsync();
    }

    public Task DisposeAsync()
    {
        _client?.Dispose();
        return Task.CompletedTask;
    }

    private async Task SeedTestDataAsync()
    {
        _adminUserId = Guid.NewGuid();
        _regularUserId = Guid.NewGuid();

        using var scope = _customFactory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var adminUser = new User
        {
            Id = _adminUserId,
            FirstName = "Admin",
            LastName = "User",
            Email = "admin@test.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var regularUser = new User
        {
            Id = _regularUserId,
            FirstName = "Regular",
            LastName = "User",
            Email = "user@test.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.AddRange(adminUser, regularUser);
        await context.SaveChangesAsync();
    }

    #region GET Tests

    [Fact]
    public async Task GET_Departments_ReturnsTreeStructure()
    {
        // Arrange
        var parentDept = new Department
        {
            Id = Guid.NewGuid(),
            Name = "Parent Department",
            Description = "Parent",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var childDept = new Department
        {
            Id = Guid.NewGuid(),
            Name = "Child Department",
            Description = "Child",
            ParentDepartmentId = parentDept.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        using (var scope = _customFactory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Departments.AddRange(parentDept, childDept);
            await context.SaveChangesAsync();
        }

        SetAuthHeader(_adminUserId, "Admin");

        // Act
        var response = await _client.GetAsync("/api/departments/tree");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var departments = await response.Content.ReadFromJsonAsync<List<DepartmentTreeDto>>();
        departments.Should().NotBeNull();
        departments.Should().HaveCount(1); // Only root departments
        departments![0].Name.Should().Be("Parent Department");
        departments[0].Children.Should().HaveCount(1);
        departments[0].Children[0].Name.Should().Be("Child Department");
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

/// <summary>
/// Test authentication handler for integration tests.
/// Uses headers to set user identity and roles.
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Test-User-Id", out var userIdHeader))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userIdHeader!),
            new Claim(ClaimTypes.Name, "Test User")
        };

        if (Request.Headers.TryGetValue("Test-User-Role", out var roleHeader))
        {
            claims.Add(new Claim(ClaimTypes.Role, roleHeader!));
        }

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
