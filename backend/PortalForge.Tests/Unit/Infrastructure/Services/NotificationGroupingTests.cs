using FluentAssertions;
using Moq;
using Xunit;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;
using PortalForge.Infrastructure.Services;

namespace PortalForge.Tests.Unit.Infrastructure.Services;

public class NotificationGroupingTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<INotificationTemplateService> _mockTemplateService;
    private readonly Mock<INotificationPreferencesRepository> _mockPreferencesRepository;
    private readonly SmartNotificationService _service;

    public NotificationGroupingTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockEmailService = new Mock<IEmailService>();
        _mockTemplateService = new Mock<INotificationTemplateService>();
        _mockPreferencesRepository = new Mock<INotificationPreferencesRepository>();

        _mockUnitOfWork.Setup(u => u.NotificationPreferencesRepository)
            .Returns(_mockPreferencesRepository.Object);

        _service = new SmartNotificationService(
            _mockUnitOfWork.Object,
            _mockEmailService.Object,
            _mockTemplateService.Object);
    }

    [Fact]
    public async Task GroupNotificationsAsync_NotificationsExceedTimeWindow_CreatesMultipleGroups()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferences = new NotificationPreferences
        {
            UserId = userId,
            GroupSimilarNotifications = true,
            MaxGroupSize = 5,
            GroupingTimeWindowMinutes = 30 // 30 minutes window
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
                CreatedAt = baseTime.AddMinutes(45) // Outside time window
            }
        };

        _mockPreferencesRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(preferences);

        // Act
        var result = await _service.GroupNotificationsAsync(userId, notifications);

        // Assert
        result.Should().HaveCount(2); // Two separate groups due to time window
        result.All(g => g.Count == 1).Should().BeTrue();
    }

    [Fact]
    public async Task GroupNotificationsAsync_NotificationsExceedMaxGroupSize_SplitsIntoMultipleGroups()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferences = new NotificationPreferences
        {
            UserId = userId,
            GroupSimilarNotifications = true,
            MaxGroupSize = 2, // Small group size
            GroupingTimeWindowMinutes = 120 // Large time window
        };

        var baseTime = DateTime.UtcNow;
        var notifications = new List<Notification>
        {
            new Notification
            {
                Id = Guid.NewGuid(),
                Type = NotificationType.RequestPendingApproval,
                Title = "Request 1",
                CreatedAt = baseTime
            },
            new Notification
            {
                Id = Guid.NewGuid(),
                Type = NotificationType.RequestPendingApproval,
                Title = "Request 2",
                CreatedAt = baseTime.AddMinutes(10)
            },
            new Notification
            {
                Id = Guid.NewGuid(),
                Type = NotificationType.RequestPendingApproval,
                Title = "Request 3",
                CreatedAt = baseTime.AddMinutes(20)
            }
        };

        _mockPreferencesRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(preferences);

        // Act
        var result = await _service.GroupNotificationsAsync(userId, notifications);

        // Assert
        result.Should().HaveCount(2); // Split into 2 groups due to max size
        result.First().Count.Should().Be(2);
        result.Last().Count.Should().Be(1);
    }

    [Fact]
    public async Task GroupNotificationsAsync_DifferentNotificationTypes_CreatesGroupsPerType()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferences = new NotificationPreferences
        {
            UserId = userId,
            GroupSimilarNotifications = true,
            MaxGroupSize = 10,
            GroupingTimeWindowMinutes = 60
        };

        var baseTime = DateTime.UtcNow;
        var notifications = new List<Notification>
        {
            new Notification
            {
                Id = Guid.NewGuid(),
                Type = NotificationType.RequestPendingApproval,
                Title = "Pending 1",
                CreatedAt = baseTime
            },
            new Notification
            {
                Id = Guid.NewGuid(),
                Type = NotificationType.RequestPendingApproval,
                Title = "Pending 2",
                CreatedAt = baseTime.AddMinutes(10)
            },
            new Notification
            {
                Id = Guid.NewGuid(),
                Type = NotificationType.RequestApproved,
                Title = "Approved 1",
                CreatedAt = baseTime.AddMinutes(5)
            },
            new Notification
            {
                Id = Guid.NewGuid(),
                Type = NotificationType.RequestRejected,
                Title = "Rejected 1",
                CreatedAt = baseTime.AddMinutes(15)
            }
        };

        _mockPreferencesRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(preferences);

        // Act
        var result = await _service.GroupNotificationsAsync(userId, notifications);

        // Assert
        result.Should().HaveCount(3); // Three different types
        
        var pendingGroup = result.First(g => g.Type == NotificationType.RequestPendingApproval);
        pendingGroup.Count.Should().Be(2);
        
        var approvedGroup = result.First(g => g.Type == NotificationType.RequestApproved);
        approvedGroup.Count.Should().Be(1);
        
        var rejectedGroup = result.First(g => g.Type == NotificationType.RequestRejected);
        rejectedGroup.Count.Should().Be(1);
    }

    [Fact]
    public async Task GroupNotificationsAsync_GroupedNotifications_GeneratesCorrectGroupTitle()
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
                Title = "New request: Vacation",
                Message = "John submitted vacation request",
                CreatedAt = baseTime
            },
            new Notification
            {
                Id = Guid.NewGuid(),
                Type = NotificationType.RequestPendingApproval,
                Title = "New request: Sick Leave",
                Message = "Jane submitted sick leave request",
                CreatedAt = baseTime.AddMinutes(10)
            },
            new Notification
            {
                Id = Guid.NewGuid(),
                Type = NotificationType.RequestPendingApproval,
                Title = "New request: Training",
                Message = "Bob submitted training request",
                CreatedAt = baseTime.AddMinutes(20)
            }
        };

        _mockPreferencesRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(preferences);

        // Act
        var result = await _service.GroupNotificationsAsync(userId, notifications);

        // Assert
        result.Should().HaveCount(1);
        var group = result.First();
        
        group.Count.Should().Be(3);
        group.Title.Should().Be("New request: Vacation (+2 więcej)");
        group.Message.Should().Contain("3 powiadomień typu oczekujące zatwierdzenie");
        group.NotificationIds.Should().HaveCount(3);
        group.FirstCreatedAt.Should().Be(baseTime);
        group.LastCreatedAt.Should().Be(baseTime.AddMinutes(20));
    }

    [Fact]
    public async Task GroupNotificationsAsync_SingleNotification_KeepsOriginalTitleAndMessage()
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

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            Type = NotificationType.RequestApproved,
            Title = "Request approved: Vacation",
            Message = "Your vacation request has been approved by John Manager",
            ActionUrl = "/requests/123",
            CreatedAt = DateTime.UtcNow
        };

        _mockPreferencesRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(preferences);

        // Act
        var result = await _service.GroupNotificationsAsync(userId, new List<Notification> { notification });

        // Assert
        result.Should().HaveCount(1);
        var group = result.First();
        
        group.Count.Should().Be(1);
        group.Title.Should().Be("Request approved: Vacation");
        group.Message.Should().Be("Your vacation request has been approved by John Manager");
        group.ActionUrl.Should().Be("/requests/123");
        group.NotificationIds.Should().Contain(notification.Id);
    }

    [Fact]
    public async Task GroupNotificationsAsync_MultipleNotifications_UsesGenericActionUrl()
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

        var notifications = new List<Notification>
        {
            new Notification
            {
                Id = Guid.NewGuid(),
                Type = NotificationType.RequestApproved,
                Title = "Request 1 approved",
                ActionUrl = "/requests/123",
                CreatedAt = DateTime.UtcNow
            },
            new Notification
            {
                Id = Guid.NewGuid(),
                Type = NotificationType.RequestApproved,
                Title = "Request 2 approved",
                ActionUrl = "/requests/456",
                CreatedAt = DateTime.UtcNow.AddMinutes(5)
            }
        };

        _mockPreferencesRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(preferences);

        // Act
        var result = await _service.GroupNotificationsAsync(userId, notifications);

        // Assert
        result.Should().HaveCount(1);
        var group = result.First();
        
        group.Count.Should().Be(2);
        group.ActionUrl.Should().Be("/dashboard/notifications"); // Generic URL for multiple notifications
    }

    [Fact]
    public async Task GroupNotificationsAsync_EmptyNotificationList_ReturnsEmptyGroups()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferences = new NotificationPreferences
        {
            UserId = userId,
            GroupSimilarNotifications = true
        };

        _mockPreferencesRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(preferences);

        // Act
        var result = await _service.GroupNotificationsAsync(userId, new List<Notification>());

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(NotificationType.RequestPendingApproval, "oczekujące zatwierdzenie")]
    [InlineData(NotificationType.RequestApproved, "zatwierdzone wnioski")]
    [InlineData(NotificationType.RequestRejected, "odrzucone wnioski")]
    [InlineData(NotificationType.VacationCoverageAssigned, "przypisane zastępstwa")]
    [InlineData(NotificationType.ApprovalOverdue, "przeterminowane zatwierdzenia")]
    [InlineData(NotificationType.System, "systemowe")]
    public async Task GroupNotificationsAsync_VariousNotificationTypes_GeneratesCorrectDisplayNames(
        NotificationType type, 
        string expectedDisplayName)
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

        var notifications = new List<Notification>
        {
            new Notification
            {
                Id = Guid.NewGuid(),
                Type = type,
                Title = "Test notification 1",
                CreatedAt = DateTime.UtcNow
            },
            new Notification
            {
                Id = Guid.NewGuid(),
                Type = type,
                Title = "Test notification 2",
                CreatedAt = DateTime.UtcNow.AddMinutes(5)
            }
        };

        _mockPreferencesRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(preferences);

        // Act
        var result = await _service.GroupNotificationsAsync(userId, notifications);

        // Assert
        result.Should().HaveCount(1);
        var group = result.First();
        
        group.Message.Should().Contain(expectedDisplayName);
    }
}