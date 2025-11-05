using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;
using PortalForge.Infrastructure.Persistence;
using PortalForge.Infrastructure.Repositories;
using Xunit;

namespace PortalForge.Tests.Integration;

/// <summary>
/// Integration tests for enhanced request system database changes and repository methods.
/// Tests new entities, enhanced repository methods, and database migrations.
/// </summary>
public class EnhancedRequestSystemRepositoryTests : IAsyncLifetime
{
    private ServiceProvider _serviceProvider = null!;
    private ApplicationDbContext _context = null!;
    private IUnitOfWork _unitOfWork = null!;

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

        // Add Unit of Work and repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<ApplicationDbContext>();
        _unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();

        // Seed test data
        await SeedTestDataAsync();
    }

    public Task DisposeAsync()
    {
        _serviceProvider?.Dispose();
        return Task.CompletedTask;
    }

    private async Task SeedTestDataAsync()
    {
        // Create test users
        var users = new[]
        {
            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@test.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.Users.AddRange(users);

        // Create test request template
        var template = new RequestTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Test Template",
            Description = "Test Description",
            Category = "Test",
            ServiceCategory = "IT Support",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedById = users[0].Id
        };

        _context.RequestTemplates.Add(template);

        // Create test requests
        var requests = new[]
        {
            new Request
            {
                Id = Guid.NewGuid(),
                RequestNumber = "REQ-001",
                RequestTemplateId = template.Id,
                SubmittedById = users[0].Id,
                SubmittedAt = DateTime.UtcNow,
                FormData = "{}",
                Status = RequestStatus.InReview,
                ServiceCategory = "IT Support",
                Tags = "[\"urgent\", \"hardware\"]"
            },
            new Request
            {
                Id = Guid.NewGuid(),
                RequestNumber = "REQ-002",
                RequestTemplateId = template.Id,
                SubmittedById = users[1].Id,
                SubmittedAt = DateTime.UtcNow.AddDays(-1),
                FormData = "{}",
                Status = RequestStatus.Approved,
                IsTemplate = true,
                Tags = "[\"template\", \"standard\"]"
            }
        };

        _context.Requests.AddRange(requests);
        await _context.SaveChangesAsync();
    }

    #region RequestAnalytics Tests

    [Fact]
    public async Task RequestAnalytics_CreateAndRetrieve_ShouldWork()
    {
        // Arrange
        var userId = _context.Users.First().Id;
        var analytics = new RequestAnalytics
        {
            Id = Guid.NewGuid(),
            UserId = userId,
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
        await _unitOfWork.RequestAnalyticsRepository.CreateAsync(analytics);

        // Assert
        var retrieved = await _unitOfWork.RequestAnalyticsRepository.GetByIdAsync(analytics.Id);
        retrieved.Should().NotBeNull();
        retrieved!.UserId.Should().Be(userId);
        retrieved.Year.Should().Be(2024);
        retrieved.Month.Should().Be(11);
        retrieved.TotalRequests.Should().Be(10);
        retrieved.AverageProcessingTime.Should().Be(24.5);
    }

    [Fact]
    public async Task RequestAnalytics_GetByUserAndPeriod_ShouldReturnCorrectRecord()
    {
        // Arrange
        var userId = _context.Users.First().Id;
        var analytics = new RequestAnalytics
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Year = 2024,
            Month = 10,
            TotalRequests = 5,
            ApprovedRequests = 4,
            RejectedRequests = 1,
            PendingRequests = 0,
            AverageProcessingTime = 12.0,
            LastCalculated = DateTime.UtcNow
        };

        await _unitOfWork.RequestAnalyticsRepository.CreateAsync(analytics);

        // Act
        var retrieved = await _unitOfWork.RequestAnalyticsRepository.GetByUserAndPeriodAsync(userId, 2024, 10);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.TotalRequests.Should().Be(5);
        retrieved.ApprovedRequests.Should().Be(4);
    }

    #endregion

    #region ApprovalDelegation Tests

    [Fact]
    public async Task ApprovalDelegation_CreateAndRetrieve_ShouldWork()
    {
        // Arrange
        var users = _context.Users.ToList();
        var delegation = new ApprovalDelegation
        {
            Id = Guid.NewGuid(),
            FromUserId = users[0].Id,
            ToUserId = users[1].Id,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(7),
            IsActive = true,
            Reason = "Vacation",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await _unitOfWork.ApprovalDelegationRepository.AddAsync(delegation);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var retrieved = await _unitOfWork.ApprovalDelegationRepository.GetByIdAsync(delegation.Id);
        retrieved.Should().NotBeNull();
        retrieved!.FromUserId.Should().Be(users[0].Id);
        retrieved.ToUserId.Should().Be(users[1].Id);
        retrieved.Reason.Should().Be("Vacation");
        retrieved.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task ApprovalDelegation_GetActiveDelegationsFromUser_ShouldReturnOnlyActive()
    {
        // Arrange
        var users = _context.Users.ToList();
        var activeDelegation = new ApprovalDelegation
        {
            Id = Guid.NewGuid(),
            FromUserId = users[0].Id,
            ToUserId = users[1].Id,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(5),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var inactiveDelegation = new ApprovalDelegation
        {
            Id = Guid.NewGuid(),
            FromUserId = users[0].Id,
            ToUserId = users[1].Id,
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(-5),
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ApprovalDelegationRepository.AddAsync(activeDelegation);
        await _unitOfWork.ApprovalDelegationRepository.AddAsync(inactiveDelegation);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var activeDelegations = await _unitOfWork.ApprovalDelegationRepository.GetActiveDelegationsFromUserAsync(users[0].Id);

        // Assert
        activeDelegations.Should().HaveCount(1);
        activeDelegations.First().Id.Should().Be(activeDelegation.Id);
    }

    #endregion

    #region NotificationPreferences Tests

    [Fact]
    public async Task NotificationPreferences_CreateAndRetrieve_ShouldWork()
    {
        // Arrange
        var userId = _context.Users.First().Id;
        var preferences = new NotificationPreferences
        {
            Id = Guid.NewGuid(),
            UserId = userId,
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
        await _unitOfWork.NotificationPreferencesRepository.CreateAsync(preferences);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var retrieved = await _unitOfWork.NotificationPreferencesRepository.GetByUserIdAsync(userId);
        retrieved.Should().NotBeNull();
        retrieved!.EmailEnabled.Should().BeTrue();
        retrieved.DigestEnabled.Should().BeFalse();
        retrieved.DisabledTypes.Should().Be("[\"RequestApproved\"]");
    }

    [Fact]
    public async Task NotificationPreferences_GetUsersWithDigestEnabled_ShouldReturnCorrectUsers()
    {
        // Arrange
        var users = _context.Users.ToList();
        var preferences1 = new NotificationPreferences
        {
            Id = Guid.NewGuid(),
            UserId = users[0].Id,
            DigestEnabled = true,
            DigestFrequency = DigestFrequency.Daily,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var preferences2 = new NotificationPreferences
        {
            Id = Guid.NewGuid(),
            UserId = users[1].Id,
            DigestEnabled = false,
            DigestFrequency = DigestFrequency.Daily,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.NotificationPreferencesRepository.CreateAsync(preferences1);
        await _unitOfWork.NotificationPreferencesRepository.CreateAsync(preferences2);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var usersWithDigest = await _unitOfWork.NotificationPreferencesRepository.GetUsersWithDigestEnabledAsync(DigestFrequency.Daily);

        // Assert
        usersWithDigest.Should().HaveCount(1);
        usersWithDigest.Should().Contain(users[0].Id);
        usersWithDigest.Should().NotContain(users[1].Id);
    }

    #endregion

    #region Enhanced Request Repository Tests

    [Fact]
    public async Task RequestRepository_GetByServiceCategory_ShouldReturnCorrectRequests()
    {
        // Act
        var requests = await _unitOfWork.RequestRepository.GetByServiceCategoryAsync("IT Support");

        // Assert
        requests.Should().HaveCount(1);
        requests.First().ServiceCategory.Should().Be("IT Support");
    }

    [Fact]
    public async Task RequestRepository_GetTemplateRequests_ShouldReturnOnlyTemplates()
    {
        // Act
        var templateRequests = await _unitOfWork.RequestRepository.GetTemplateRequestsAsync();

        // Assert
        templateRequests.Should().HaveCount(1);
        templateRequests.First().IsTemplate.Should().BeTrue();
    }

    [Fact]
    public async Task RequestRepository_GetByTags_ShouldReturnMatchingRequests()
    {
        // Act
        var requestsWithUrgentTag = await _unitOfWork.RequestRepository.GetByTagsAsync(new List<string> { "urgent" });

        // Assert
        requestsWithUrgentTag.Should().HaveCount(1);
        requestsWithUrgentTag.First().Tags.Should().Contain("urgent");
    }

    [Fact]
    public async Task RequestRepository_SearchRequests_ShouldReturnFilteredResults()
    {
        // Act
        var searchResults = await _unitOfWork.RequestRepository.SearchRequestsAsync(
            searchTerm: "REQ-001",
            statusFilter: new List<RequestStatus> { RequestStatus.InReview });

        // Assert
        searchResults.Should().HaveCount(1);
        searchResults.First().RequestNumber.Should().Be("REQ-001");
        searchResults.First().Status.Should().Be(RequestStatus.InReview);
    }

    [Fact]
    public async Task RequestRepository_SearchRequests_WithDateFilter_ShouldWork()
    {
        // Act
        var searchResults = await _unitOfWork.RequestRepository.SearchRequestsAsync(
            submittedAfter: DateTime.UtcNow.AddDays(-2),
            submittedBefore: DateTime.UtcNow.AddHours(-1));

        // Assert
        searchResults.Should().HaveCount(1);
        searchResults.First().RequestNumber.Should().Be("REQ-002");
    }

    #endregion

    #region Enhanced RequestTemplate Repository Tests

    [Fact]
    public async Task RequestTemplateRepository_GetByServiceCategory_ShouldReturnCorrectTemplates()
    {
        // Act
        var templates = await _unitOfWork.RequestTemplateRepository.GetByServiceCategoryAsync("IT Support");

        // Assert
        templates.Should().HaveCount(1);
        templates.First().ServiceCategory.Should().Be("IT Support");
    }

    [Fact]
    public async Task RequestTemplateRepository_GetAdvancedTemplates_ShouldReturnTemplatesWithEnhancedFeatures()
    {
        // Arrange - Add a template field with validation rules
        var template = _context.RequestTemplates.First();
        var field = new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = template.Id,
            Label = "Advanced Field",
            FieldType = FieldType.Text,
            ValidationRules = "{\"required\": true, \"minLength\": 5}",
            Order = 1
        };

        _context.RequestTemplateFields.Add(field);
        await _context.SaveChangesAsync();

        // Act
        var advancedTemplates = await _unitOfWork.RequestTemplateRepository.GetAdvancedTemplatesAsync();

        // Assert
        advancedTemplates.Should().HaveCount(1);
        advancedTemplates.First().Id.Should().Be(template.Id);
    }

    [Fact]
    public async Task RequestTemplateRepository_GetByFieldTypes_ShouldReturnMatchingTemplates()
    {
        // Arrange - Add a template field with specific field type
        var template = _context.RequestTemplates.First();
        var field = new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = template.Id,
            Label = "File Upload Field",
            FieldType = FieldType.FileUpload,
            Order = 1
        };

        _context.RequestTemplateFields.Add(field);
        await _context.SaveChangesAsync();

        // Act
        var templatesWithFileUpload = await _unitOfWork.RequestTemplateRepository.GetByFieldTypesAsync(
            new List<FieldType> { FieldType.FileUpload });

        // Assert
        templatesWithFileUpload.Should().HaveCount(1);
        templatesWithFileUpload.First().Id.Should().Be(template.Id);
    }

    #endregion

    #region Database Migration Tests

    [Fact]
    public async Task Database_NewEntities_ShouldBeCreatedSuccessfully()
    {
        // Test that all new entities can be created and saved
        var userId = _context.Users.First().Id;

        // Test RequestAnalytics
        var analytics = new RequestAnalytics
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Year = 2024,
            Month = 12,
            TotalRequests = 1,
            ApprovedRequests = 1,
            RejectedRequests = 0,
            PendingRequests = 0,
            AverageProcessingTime = 1.0,
            LastCalculated = DateTime.UtcNow
        };

        // Test ApprovalDelegation
        var delegation = new ApprovalDelegation
        {
            Id = Guid.NewGuid(),
            FromUserId = userId,
            ToUserId = userId,
            StartDate = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Test NotificationPreferences
        var preferences = new NotificationPreferences
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act & Assert - Should not throw exceptions
        _context.RequestAnalytics.Add(analytics);
        _context.ApprovalDelegations.Add(delegation);
        _context.NotificationPreferences.Add(preferences);
        
        var result = await _context.SaveChangesAsync();
        result.Should().Be(3); // 3 entities saved
    }

    [Fact]
    public async Task Database_EnhancedRequestProperties_ShouldBePersistedCorrectly()
    {
        // Arrange
        var template = _context.RequestTemplates.First();
        var user = _context.Users.First();
        
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
        await _context.SaveChangesAsync();

        // Assert
        var savedRequest = await _context.Requests.FindAsync(request.Id);
        savedRequest.Should().NotBeNull();
        savedRequest!.ServiceCategory.Should().Be("HR Support");
        savedRequest.ServiceStatus.Should().Be(ServiceTaskStatus.InProgress);
        savedRequest.ServiceNotes.Should().Be("Processing request");
        savedRequest.Tags.Should().Be("[\"hr\", \"benefits\"]");
    }

    [Fact]
    public async Task Database_EnhancedRequestTemplateFieldProperties_ShouldBePersistedCorrectly()
    {
        // Arrange
        var template = _context.RequestTemplates.First();
        
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
        await _context.SaveChangesAsync();

        // Assert
        var savedField = await _context.RequestTemplateFields.FindAsync(field.Id);
        savedField.Should().NotBeNull();
        savedField!.ValidationRules.Should().Be("{\"required\": true, \"minLength\": 3}");
        savedField.ConditionalLogic.Should().Be("{\"showIf\": {\"field\": \"type\", \"value\": \"urgent\"}}");
        savedField.IsConditional.Should().BeTrue();
        savedField.DefaultValue.Should().Be("Default text");
        savedField.AutoCompleteSource.Should().Be("/api/suggestions");
        savedField.FileMaxSize.Should().Be(5242880);
        savedField.AllowedFileTypes.Should().Be("[\"pdf\", \"doc\", \"docx\"]");
    }

    #endregion
}