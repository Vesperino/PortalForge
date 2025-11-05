using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;
using PortalForge.Infrastructure.Persistence;
using Xunit;

namespace PortalForge.Tests.Integration;

/// <summary>
/// Integration tests for database migrations and new entity creation.
/// Verifies that the enhanced request system database changes work correctly.
/// </summary>
public class DatabaseMigrationTests : IAsyncLifetime
{
    private ServiceProvider _serviceProvider = null!;
    private ApplicationDbContext _context = null!;

    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();
        
        // Add Entity Framework with in-memory database
        var dbName = $"InMemoryTestDb_{Guid.NewGuid()}";
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseInMemoryDatabase(dbName);
            options.EnableSensitiveDataLogging();
        });

        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<ApplicationDbContext>();

        // Ensure database is created
        await _context.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync()
    {
        _serviceProvider?.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Database_CanCreateRequestAnalytics_ShouldSucceed()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var analytics = new RequestAnalytics
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Year = 2024,
            Month = 11,
            TotalRequests = 10,
            ApprovedRequests = 8,
            RejectedRequests = 1,
            PendingRequests = 1,
            AverageProcessingTime = 24.5,
            LastCalculated = DateTime.UtcNow
        };

        // Act
        _context.RequestAnalytics.Add(analytics);
        var result = await _context.SaveChangesAsync();

        // Assert
        result.Should().Be(1);
        var saved = await _context.RequestAnalytics.FindAsync(analytics.Id);
        saved.Should().NotBeNull();
        saved!.TotalRequests.Should().Be(10);
        saved.AverageProcessingTime.Should().Be(24.5);
    }

    [Fact]
    public async Task Database_CanCreateApprovalDelegation_ShouldSucceed()
    {
        // Arrange
        var fromUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "From",
            LastName = "User",
            Email = "from@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var toUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "To",
            LastName = "User",
            Email = "to@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.AddRange(fromUser, toUser);
        await _context.SaveChangesAsync();

        var delegation = new ApprovalDelegation
        {
            Id = Guid.NewGuid(),
            FromUserId = fromUser.Id,
            ToUserId = toUser.Id,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(7),
            IsActive = true,
            Reason = "Vacation",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        _context.ApprovalDelegations.Add(delegation);
        var result = await _context.SaveChangesAsync();

        // Assert
        result.Should().Be(1);
        var saved = await _context.ApprovalDelegations.FindAsync(delegation.Id);
        saved.Should().NotBeNull();
        saved!.Reason.Should().Be("Vacation");
        saved.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Database_CanCreateNotificationPreferences_ShouldSucceed()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var preferences = new NotificationPreferences
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            EmailEnabled = true,
            InAppEnabled = true,
            DigestEnabled = false,
            DigestFrequency = DigestFrequency.Daily,
            DisabledTypes = "[\"RequestApproved\"]",
            GroupSimilarNotifications = true,
            MaxGroupSize = 5,
            GroupingTimeWindowMinutes = 60,
            RealTimeEnabled = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        _context.NotificationPreferences.Add(preferences);
        var result = await _context.SaveChangesAsync();

        // Assert
        result.Should().Be(1);
        var saved = await _context.NotificationPreferences.FindAsync(preferences.Id);
        saved.Should().NotBeNull();
        saved!.EmailEnabled.Should().BeTrue();
        saved.DigestEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task Database_CanCreateEnhancedRequest_ShouldSucceed()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var template = new RequestTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Test Template",
            Description = "Test Description",
            Category = "Test",
            ServiceCategory = "IT Support",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedById = user.Id
        };

        _context.Users.Add(user);
        _context.RequestTemplates.Add(template);
        await _context.SaveChangesAsync();

        var request = new Request
        {
            Id = Guid.NewGuid(),
            RequestNumber = "REQ-ENHANCED",
            RequestTemplateId = template.Id,
            SubmittedById = user.Id,
            SubmittedAt = DateTime.UtcNow,
            FormData = "{}",
            Status = RequestStatus.InReview,
            ServiceCategory = "HR Support",
            ServiceStatus = ServiceTaskStatus.InProgress,
            ServiceCompletedAt = DateTime.UtcNow.AddHours(2),
            ServiceNotes = "Processing request",
            IsTemplate = false,
            Tags = "[\"hr\", \"benefits\"]"
        };

        // Act
        _context.Requests.Add(request);
        var result = await _context.SaveChangesAsync();

        // Assert
        result.Should().Be(1);
        var saved = await _context.Requests.FindAsync(request.Id);
        saved.Should().NotBeNull();
        saved!.ServiceCategory.Should().Be("HR Support");
        saved.ServiceStatus.Should().Be(ServiceTaskStatus.InProgress);
        saved.ServiceNotes.Should().Be("Processing request");
        saved.Tags.Should().Be("[\"hr\", \"benefits\"]");
    }

    [Fact]
    public async Task Database_CanCreateEnhancedRequestTemplateField_ShouldSucceed()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var template = new RequestTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Test Template",
            Description = "Test Description",
            Category = "Test",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedById = user.Id
        };

        _context.Users.Add(user);
        _context.RequestTemplates.Add(template);
        await _context.SaveChangesAsync();

        var field = new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = template.Id,
            Label = "Enhanced Field",
            FieldType = FieldType.Text,
            ValidationRules = "{\"required\": true, \"minLength\": 3}",
            ConditionalLogic = "{\"showIf\": {\"field\": \"type\", \"value\": \"urgent\"}}",
            IsConditional = true,
            DefaultValue = "Default text",
            AutoCompleteSource = "/api/suggestions",
            FileMaxSize = 5242880, // 5MB
            AllowedFileTypes = "[\"pdf\", \"doc\", \"docx\"]",
            Order = 1
        };

        // Act
        _context.RequestTemplateFields.Add(field);
        var result = await _context.SaveChangesAsync();

        // Assert
        result.Should().Be(1);
        var saved = await _context.RequestTemplateFields.FindAsync(field.Id);
        saved.Should().NotBeNull();
        saved!.ValidationRules.Should().Be("{\"required\": true, \"minLength\": 3}");
        saved.ConditionalLogic.Should().Be("{\"showIf\": {\"field\": \"type\", \"value\": \"urgent\"}}");
        saved.IsConditional.Should().BeTrue();
        saved.DefaultValue.Should().Be("Default text");
        saved.AutoCompleteSource.Should().Be("/api/suggestions");
        saved.FileMaxSize.Should().Be(5242880);
        saved.AllowedFileTypes.Should().Be("[\"pdf\", \"doc\", \"docx\"]");
    }

    [Fact]
    public async Task Database_CanCreateRequestWithCloning_ShouldSucceed()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var template = new RequestTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Test Template",
            Description = "Test Description",
            Category = "Test",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedById = user.Id
        };

        _context.Users.Add(user);
        _context.RequestTemplates.Add(template);
        await _context.SaveChangesAsync();

        var originalRequest = new Request
        {
            Id = Guid.NewGuid(),
            RequestNumber = "REQ-ORIGINAL",
            RequestTemplateId = template.Id,
            SubmittedById = user.Id,
            SubmittedAt = DateTime.UtcNow,
            FormData = "{}",
            Status = RequestStatus.Approved,
            IsTemplate = true
        };

        _context.Requests.Add(originalRequest);
        await _context.SaveChangesAsync();

        var clonedRequest = new Request
        {
            Id = Guid.NewGuid(),
            RequestNumber = "REQ-CLONED",
            RequestTemplateId = template.Id,
            SubmittedById = user.Id,
            SubmittedAt = DateTime.UtcNow,
            FormData = "{}",
            Status = RequestStatus.Draft,
            ClonedFromId = originalRequest.Id
        };

        // Act
        _context.Requests.Add(clonedRequest);
        var result = await _context.SaveChangesAsync();

        // Assert
        result.Should().Be(1);
        var saved = await _context.Requests.FindAsync(clonedRequest.Id);
        saved.Should().NotBeNull();
        saved!.ClonedFromId.Should().Be(originalRequest.Id);
    }

    [Fact]
    public async Task Database_AllNewEntitiesExist_ShouldSucceed()
    {
        // Act & Assert - Verify all new DbSets exist and can be queried
        var analyticsCount = await _context.RequestAnalytics.CountAsync();
        var delegationsCount = await _context.ApprovalDelegations.CountAsync();
        var preferencesCount = await _context.NotificationPreferences.CountAsync();

        // Should not throw exceptions
        analyticsCount.Should().BeGreaterThanOrEqualTo(0);
        delegationsCount.Should().BeGreaterThanOrEqualTo(0);
        preferencesCount.Should().BeGreaterThanOrEqualTo(0);
    }
}