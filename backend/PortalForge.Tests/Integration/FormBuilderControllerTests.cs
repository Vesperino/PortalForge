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
using PortalForge.Api.DTOs.Requests.FormBuilder;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;
using PortalForge.Infrastructure.Persistence;
using Xunit;

namespace PortalForge.Tests.Integration;

/// <summary>
/// Integration tests for FormBuilderController.
/// </summary>
public class FormBuilderControllerTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private WebApplicationFactory<Program> _customFactory = null!;
    private HttpClient _client = null!;
    private Guid _userId;
    private Guid _templateId;

    public FormBuilderControllerTests(WebApplicationFactory<Program> factory)
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
            Name = "Test Template",
            Description = "Test template for form builder",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedById = _userId
        };

        var field = new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = _templateId,
            Label = "Test Field",
            FieldType = FieldType.Text,
            IsRequired = true,
            Order = 1
        };

        context.Users.Add(user);
        context.RequestTemplates.Add(template);
        context.RequestTemplateFields.Add(field);
        await context.SaveChangesAsync();
    }

    #region Build Form Tests

    [Fact]
    public async Task GET_BuildForm_WithValidTemplate_ReturnsOk()
    {
        // Arrange
        SetAuthHeader(_userId, "User");

        // Act
        var response = await _client.GetAsync($"/api/form-builder/templates/{_templateId}/form");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GET_BuildForm_WithInvalidTemplate_ReturnsBadRequest()
    {
        // Arrange
        SetAuthHeader(_userId, "User");
        var invalidTemplateId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/form-builder/templates/{invalidTemplateId}/form");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Validate Form Data Tests

    [Fact]
    public async Task POST_ValidateFormData_WithValidData_ReturnsOk()
    {
        // Arrange
        SetAuthHeader(_userId, "User");
        var dto = new ValidateFormDataDto
        {
            FormData = "{\"testField\": \"test value\"}"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/form-builder/templates/{_templateId}/validate", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task POST_ValidateFormData_WithInvalidJson_ReturnsBadRequest()
    {
        // Arrange
        SetAuthHeader(_userId, "User");
        var dto = new ValidateFormDataDto
        {
            FormData = "invalid json"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/form-builder/templates/{_templateId}/validate", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Conditional Logic Tests

    [Fact]
    public async Task POST_ProcessConditionalLogic_WithValidData_ReturnsOk()
    {
        // Arrange
        SetAuthHeader(_userId, "User");
        var dto = new ProcessConditionalLogicDto
        {
            FormData = "{\"testField\": \"test value\"}"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/form-builder/templates/{_templateId}/conditional-logic", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Auto-complete Tests

    [Fact]
    public async Task GET_AutoCompleteOptions_WithValidSource_ReturnsOk()
    {
        // Arrange
        SetAuthHeader(_userId, "User");

        // Act
        var response = await _client.GetAsync("/api/form-builder/autocomplete/users?query=test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GET_AutoCompleteOptions_WithInvalidSource_ReturnsBadRequest()
    {
        // Arrange
        SetAuthHeader(_userId, "User");

        // Act
        var response = await _client.GetAsync("/api/form-builder/autocomplete/invalid-source");

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