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
            builder.ConfigureServices((context, services) =>
            {
                // Remove all DbContext-related services added by Infrastructure
                var descriptorsToRemove = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                                d.ServiceType == typeof(DbContextOptions) ||
                                d.ServiceType == typeof(ApplicationDbContext))
                    .ToList();

                foreach (var descriptor in descriptorsToRemove)
                {
                    services.Remove(descriptor);
                }
            });

            builder.ConfigureTestServices(services =>
            {
                // Add in-memory database
                var dbName = $"InMemoryTestDb_{Guid.NewGuid()}";
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                });

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

    #region POST Tests

    [Fact]
    public async Task POST_Department_CreatesSuccessfully_WhenAdminRole()
    {
        // Arrange
        SetAuthHeader(_adminUserId, "Admin");

        var createDto = new CreateDepartmentDto
        {
            Name = "New Department",
            Description = "Test Description",
            ParentDepartmentId = null,
            DepartmentHeadId = null
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/departments", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdDept = await response.Content.ReadFromJsonAsync<DepartmentDto>();
        createdDept.Should().NotBeNull();
        createdDept!.Name.Should().Be("New Department");

        // Verify in database
        using (var scope = _customFactory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var dbDept = await context.Departments.FirstOrDefaultAsync(d => d.Name == "New Department");
            dbDept.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task POST_Department_ReturnsForbidden_WhenNotAdmin()
    {
        // Arrange
        SetAuthHeader(_regularUserId, "Employee");

        var createDto = new CreateDepartmentDto
        {
            Name = "New Department",
            Description = "Test"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/departments", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task POST_Department_ValidatesParentExists()
    {
        // Arrange
        SetAuthHeader(_adminUserId, "Admin");

        var createDto = new CreateDepartmentDto
        {
            Name = "New Department",
            Description = "Test",
            ParentDepartmentId = Guid.NewGuid() // Non-existent parent
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/departments", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_Department_PreventsCircularReferences()
    {
        // Arrange
        SetAuthHeader(_adminUserId, "Admin");

        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var parent = new Department
        {
            Id = parentId,
            Name = "Parent",
            Description = "Parent",
            CreatedAt = DateTime.UtcNow
        };

        var child = new Department
        {
            Id = childId,
            Name = "Child",
            Description = "Child",
            ParentDepartmentId = parentId,
            CreatedAt = DateTime.UtcNow
        };

        using (var scope = _customFactory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Departments.AddRange(parent, child);
            await context.SaveChangesAsync();
        }

        // Act - Try to make parent a child of child (circular)
        var updateDto = new UpdateDepartmentDto
        {
            Name = "Parent",
            Description = "Parent",
            ParentDepartmentId = childId // Circular reference!
        };

        var response = await _client.PutAsJsonAsync($"/api/departments/{parentId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadAsStringAsync();
        error.Should().Contain("circular");
    }

    #endregion

    #region PUT Tests

    [Fact]
    public async Task PUT_Department_UpdatesSuccessfully()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var department = new Department
        {
            Id = departmentId,
            Name = "Old Name",
            Description = "Old Description",
            CreatedAt = DateTime.UtcNow
        };

        using (var scope = _customFactory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Departments.Add(department);
            await context.SaveChangesAsync();
        }

        SetAuthHeader(_adminUserId, "Admin");

        var updateDto = new UpdateDepartmentDto
        {
            Name = "New Name",
            Description = "New Description"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/departments/{departmentId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify in database
        using (var scope = _customFactory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var updatedDept = await context.Departments.FindAsync(departmentId);
            updatedDept.Should().NotBeNull();
            updatedDept!.Name.Should().Be("New Name");
            updatedDept.Description.Should().Be("New Description");
        }
    }

    #endregion

    #region DELETE Tests

    [Fact]
    public async Task DELETE_Department_SoftDeletes()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var department = new Department
        {
            Id = departmentId,
            Name = "To Delete",
            Description = "Will be soft deleted",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        using (var scope = _customFactory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Departments.Add(department);
            await context.SaveChangesAsync();
        }

        SetAuthHeader(_adminUserId, "Admin");

        // Act
        var response = await _client.DeleteAsync($"/api/departments/{departmentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify soft delete in database (IsActive set to false)
        using (var scope = _customFactory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var deletedDept = await context.Departments
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(d => d.Id == departmentId);

            deletedDept.Should().NotBeNull();
            deletedDept!.IsActive.Should().BeFalse();
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
