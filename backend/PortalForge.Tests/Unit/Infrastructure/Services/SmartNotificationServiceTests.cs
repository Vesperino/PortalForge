using FluentAssertions;
using Moq;
using Xunit;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.DTOs;
using PortalForge.Application.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;
using PortalForge.Infrastructure.Services;

namespace PortalForge.Tests.Unit.Infrastructure.Services;

public class SmartNotificationServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<INotificationTemplateService> _mockTemplateService;
    private readonly Mock<INotificationPreferencesRepository> _mockPreferencesRepository;
    private readonly Mock<INotificationRepository> _mockNotificationRepository;
    private readonly SmartNotificationService _service;

    public SmartNotificationServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockEmailService = new Mock<IEmailService>();
        _mockTemplateService = new Mock<INotificationTemplateService>();
        _mockPreferencesRepository = new Mock<INotificationPreferencesRepository>();
        _mockNotificationRepository = new Mock<INotificationRepository>();

        _mockUnitOfWork.Setup(u => u.NotificationPreferencesRepository)
            .Returns(_mockPreferencesRepository.Object);
        _mockUnitOfWork.Setup(u => u.NotificationRepository)
            .Returns(_mockNotificationRepository.Object);

        _service = new SmartNotificationService(
            _mockUnitOfWork.Object,
            _mockEmailService.Object,
            _mockTemplateService.Object);
    }

    [Fact]
    public async Task GetUserPreferencesAsync_ExistingPreferences_ReturnsPreferences()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferences = new NotificationPreferences
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EmailEnabled = true,
            InAppEnabled = true,
            DigestEnabled = false
        };

        _mockPreferencesRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(preferences);

        // Act
        var result = await _service.GetUserPreferencesAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.EmailEnabled.Should().BeTrue();
        result.InAppEnabled.Should().BeTrue();
        result.DigestEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserPreferencesAsync_NoExistingPreferences_CreatesDefaultPreferences()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockPreferencesRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync((NotificationPreferences?)null);

        // Act
        var result = await _service.GetUserPreferencesAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.EmailEnabled.Should().BeTrue();
        result.InAppEnabled.Should().BeTrue();
        result.DigestEnabled.Should().BeFalse();
        result.GroupSimilarNotifications.Should().BeTrue();
        result.RealTimeEnabled.Should().BeTrue();

        _mockPreferencesRepository.Verify(r => r.CreateAsync(It.IsAny<NotificationPreferences>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateUserPreferencesAsync_ExistingPreferences_UpdatesPreferences()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingPreferences = new NotificationPreferences
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EmailEnabled = true,
            InAppEnabled = true
        };

        var newPreferences = new NotificationPreferences
        {
            UserId = userId,
            EmailEnabled = false,
            InAppEnabled = true,
            DigestEnabled = true
        };

        _mockPreferencesRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(existingPreferences);

        // Act
        await _service.UpdateUserPreferencesAsync(userId, newPreferences);

        // Assert
        _mockPreferencesRepository.Verify(r => r.UpdateAsync(
            It.Is<NotificationPreferences>(p => 
                p.EmailEnabled == false &&
                p.InAppEnabled == true &&
                p.DigestEnabled == true)), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateUserPreferencesAsync_NoExistingPreferences_CreatesNewPreferences()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newPreferences = new NotificationPreferences
        {
            UserId = userId,
            EmailEnabled = false,
            DigestEnabled = true
        };

        _mockPreferencesRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync((NotificationPreferences?)null);

        // Act
        await _service.UpdateUserPreferencesAsync(userId, newPreferences);

        // Assert
        _mockPreferencesRepository.Verify(r => r.CreateAsync(
            It.Is<NotificationPreferences>(p => 
                p.UserId == userId &&
                p.EmailEnabled == false &&
                p.DigestEnabled == true)), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task IsNotificationTypeEnabledAsync_TypeNotDisabled_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferences = new NotificationPreferences
        {
            UserId = userId,
            DisabledTypes = "[]" // Empty array
        };

        _mockPreferencesRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(preferences);

        // Act
        var result = await _service.IsNotificationTypeEnabledAsync(userId, NotificationType.RequestPendingApproval);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsNotificationTypeEnabledAsync_TypeDisabled_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferences = new NotificationPreferences
        {
            UserId = userId,
            DisabledTypes = "[\"RequestPendingApproval\"]"
        };

        _mockPreferencesRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(preferences);

        // Act
        var result = await _service.IsNotificationTypeEnabledAsync(userId, NotificationType.RequestPendingApproval);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GroupNotificationsAsync_GroupingDisabled_ReturnsIndividualGroups()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferences = new NotificationPreferences
        {
            UserId = userId,
            GroupSimilarNotifications = false
        };

        var notifications = new List<Notification>
        {
            new Notification
            {
                Id = Guid.NewGuid(),
                Type = NotificationType.RequestPendingApproval,
                Title = "Request 1",
                Message = "Message 1",
                CreatedAt = DateTime.UtcNow
            },
            new Notification
            {
                Id = Guid.NewGuid(),
                Type = NotificationType.RequestPendingApproval,
                Title = "Request 2",
                Message = "Message 2",
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockPreferencesRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(preferences);

        // Act
        var result = await _service.GroupNotificationsAsync(userId, notifications);

        // Assert
        result.Should().HaveCount(2);
        result.All(g => g.Count == 1).Should().BeTrue();
    }

    [Fact]
    public async Task GroupNotificationsAsync_SimilarNotificationsWithinTimeWindow_GroupsTogether()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferences = new NotificationPreferences
        {
            UserId = userId,
            GroupSimilarNotifications = true,
            MaxGroupSize = 5,
            GroupingTimeWindowMinutes = 60
        };

        var baseTime = DateTime.UtcNow;
        var notifications = new List<Notification>
        {
            new Notification
            {
                Id = Guid.NewGuid(),
                Type = NotificationType.RequestPendingApproval,
                Title = "Request 1",
                Message = "Message 1",
                CreatedAt = baseTime
            },
            new Notification
            {
                Id = Guid.NewGuid(),
                Type = NotificationType.RequestPendingApproval,
                Title = "Request 2",
                Message = "Message 2",
                CreatedAt = baseTime.AddMinutes(30) // Within time window
            },
            new Notification
            {
                Id = Guid.NewGuid(),
                Type = NotificationType.RequestApproved,
                Title = "Approved 1",
                Message = "Approved Message",
                CreatedAt = baseTime.AddMinutes(15)
            }
        };

        _mockPreferencesRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(preferences);

        // Act
        var result = await _service.GroupNotificationsAsync(userId, notifications);

        // Assert
        result.Should().HaveCount(2); // Two different types
        var pendingApprovalGroup = result.First(g => g.Type == NotificationType.RequestPendingApproval);
        pendingApprovalGroup.Count.Should().Be(2);
        pendingApprovalGroup.NotificationIds.Should().HaveCount(2);
    }

    [Fact]
    public async Task SendGroupedNotificationsAsync_InAppDisabled_DoesNotCreateNotifications()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferences = new NotificationPreferences
        {
            UserId = userId,
            InAppEnabled = false
        };

        var groups = new List<NotificationGroupDto>
        {
            new NotificationGroupDto
            {
                Type = NotificationType.RequestPendingApproval,
                Count = 2,
                Title = "2 pending requests",
                Message = "You have 2 pending requests"
            }
        };

        _mockPreferencesRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(preferences);

        // Act
        await _service.SendGroupedNotificationsAsync(userId, groups);

        // Assert
        _mockNotificationRepository.Verify(r => r.CreateAsync(It.IsAny<Notification>()), Times.Never);
    }

    [Fact]
    public async Task SendDigestNotificationAsync_DigestDisabled_DoesNotSendDigest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferences = new NotificationPreferences
        {
            UserId = userId,
            DigestEnabled = false
        };

        _mockPreferencesRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(preferences);

        // Act
        await _service.SendDigestNotificationAsync(userId, DigestType.Daily);

        // Assert
        _mockNotificationRepository.Verify(r => r.CreateAsync(It.IsAny<Notification>()), Times.Never);
    }

    [Fact]
    public async Task CreateTemplatedNotificationAsync_TypeDisabled_DoesNotCreateNotification()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferences = new NotificationPreferences
        {
            UserId = userId,
            DisabledTypes = "[\"RequestPendingApproval\"]"
        };

        var placeholders = new Dictionary<string, object>
        {
            { "SubmitterName", "John Doe" },
            { "RequestType", "Vacation Request" }
        };

        _mockPreferencesRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(preferences);

        // Act
        await _service.CreateTemplatedNotificationAsync(
            userId, 
            NotificationType.RequestPendingApproval, 
            placeholders);

        // Assert
        _mockNotificationRepository.Verify(r => r.CreateAsync(It.IsAny<Notification>()), Times.Never);
        _mockTemplateService.Verify(t => t.RenderNotificationAsync(
            It.IsAny<NotificationType>(), 
            It.IsAny<Dictionary<string, object>>(), 
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateTemplatedNotificationAsync_TypeEnabled_CreatesNotificationWithTemplate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferences = new NotificationPreferences
        {
            UserId = userId,
            DisabledTypes = "[]",
            InAppEnabled = true,
            RealTimeEnabled = false
        };

        var placeholders = new Dictionary<string, object>
        {
            { "SubmitterName", "John Doe" },
            { "RequestType", "Vacation Request" }
        };

        var renderedTitle = "New request for approval: Vacation Request";
        var renderedMessage = "John Doe submitted a request \"Vacation Request\" waiting for your approval.";

        _mockPreferencesRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(preferences);

        _mockTemplateService
            .Setup(t => t.RenderNotificationAsync(
                NotificationType.RequestPendingApproval, 
                placeholders, 
                "pl"))
            .ReturnsAsync((renderedTitle, renderedMessage));

        // Act
        await _service.CreateTemplatedNotificationAsync(
            userId, 
            NotificationType.RequestPendingApproval, 
            placeholders,
            "Request",
            "123",
            "/requests/123");

        // Assert
        _mockTemplateService.Verify(t => t.RenderNotificationAsync(
            NotificationType.RequestPendingApproval, 
            placeholders, 
            "pl"), Times.Once);

        _mockNotificationRepository.Verify(r => r.CreateAsync(
            It.Is<Notification>(n => 
                n.UserId == userId &&
                n.Type == NotificationType.RequestPendingApproval &&
                n.Title == renderedTitle &&
                n.Message == renderedMessage &&
                n.RelatedEntityType == "Request" &&
                n.RelatedEntityId == "123" &&
                n.ActionUrl == "/requests/123")), Times.Once);
    }

    [Fact]
    public async Task GenerateDigestAsync_NoNotificationsInPeriod_ReturnsEmptyDigest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var periodStart = DateTime.UtcNow.Date.AddDays(-1);
        var periodEnd = DateTime.UtcNow.Date;

        _mockNotificationRepository
            .Setup(r => r.GetUserNotificationsAsync(userId, false, 1, 1000))
            .ReturnsAsync(new List<Notification>());

        var preferences = new NotificationPreferences
        {
            UserId = userId,
            GroupSimilarNotifications = true
        };

        _mockPreferencesRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(preferences);

        // Act
        var result = await _service.GenerateDigestAsync(userId, periodStart, periodEnd);

        // Assert
        result.Should().NotBeNull();
        result.TotalNotifications.Should().Be(0);
        result.Groups.Should().BeEmpty();
        result.Summary.Should().Contain("Brak nowych powiadomień");
    }

    [Fact]
    public async Task GenerateDigestAsync_NotificationsInPeriod_ReturnsDigestWithGroups()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var periodStart = DateTime.UtcNow.Date.AddDays(-1);
        var periodEnd = DateTime.UtcNow.Date;

        var notifications = new List<Notification>
        {
            new Notification
            {
                Id = Guid.NewGuid(),
                Type = NotificationType.RequestPendingApproval,
                Title = "Request 1",
                Message = "Message 1",
                CreatedAt = periodStart.AddHours(2)
            },
            new Notification
            {
                Id = Guid.NewGuid(),
                Type = NotificationType.RequestPendingApproval,
                Title = "Request 2",
                Message = "Message 2",
                CreatedAt = periodStart.AddHours(3)
            },
            new Notification
            {
                Id = Guid.NewGuid(),
                Type = NotificationType.RequestApproved,
                Title = "Approved",
                Message = "Request approved",
                CreatedAt = periodStart.AddHours(1)
            }
        };

        var preferences = new NotificationPreferences
        {
            UserId = userId,
            GroupSimilarNotifications = true,
            MaxGroupSize = 5,
            GroupingTimeWindowMinutes = 60
        };

        _mockNotificationRepository
            .Setup(r => r.GetUserNotificationsAsync(userId, false, 1, 1000))
            .ReturnsAsync(notifications);

        _mockPreferencesRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(preferences);

        // Act
        var result = await _service.GenerateDigestAsync(userId, periodStart, periodEnd);

        // Assert
        result.Should().NotBeNull();
        result.TotalNotifications.Should().Be(3);
        result.Groups.Should().HaveCount(2);
        result.Summary.Should().Contain("3 powiadomień");
        result.PeriodStart.Should().Be(periodStart);
        result.PeriodEnd.Should().Be(periodEnd);
    }
}