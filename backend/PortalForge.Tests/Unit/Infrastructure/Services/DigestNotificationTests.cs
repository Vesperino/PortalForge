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

public class DigestNotificationTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<INotificationTemplateService> _mockTemplateService;
    private readonly Mock<INotificationPreferencesRepository> _mockPreferencesRepository;
    private readonly Mock<INotificationRepository> _mockNotificationRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly SmartNotificationService _service;

    public DigestNotificationTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockEmailService = new Mock<IEmailService>();
        _mockTemplateService = new Mock<INotificationTemplateService>();
        _mockPreferencesRepository = new Mock<INotificationPreferencesRepository>();
        _mockNotificationRepository = new Mock<INotificationRepository>();
        _mockUserRepository = new Mock<IUserRepository>();

        _mockUnitOfWork.Setup(u => u.NotificationPreferencesRepository)
            .Returns(_mockPreferencesRepository.Object);
        _mockUnitOfWork.Setup(u => u.NotificationRepository)
            .Returns(_mockNotificationRepository.Object);
        _mockUnitOfWork.Setup(u => u.UserRepository)
            .Returns(_mockUserRepository.Object);

        _service = new SmartNotificationService(
            _mockUnitOfWork.Object,
            _mockEmailService.Object,
            _mockTemplateService.Object);
    }

    [Fact]
    public async Task SendDigestNotificationAsync_DigestEnabled_CreatesDigestNotification()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferences = new NotificationPreferences
        {
            UserId = userId,
            DigestEnabled = true,
            EmailEnabled = true
        };

        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var notifications = new List<Notification>
        {
            new Notification
            {
                Id = Guid.NewGuid(),
                Type = NotificationType.RequestPendingApproval,
                Title = "Request 1",
                CreatedAt = DateTime.UtcNow.Date.AddHours(-2)
            },
            new Notification
            {
                Id = Guid.NewGuid(),
                Type = NotificationType.RequestApproved,
                Title = "Request 2 approved",
                CreatedAt = DateTime.UtcNow.Date.AddHours(-1)
            }
        };

        _mockPreferencesRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(preferences);

        _mockNotificationRepository
            .Setup(r => r.GetUserNotificationsAsync(userId, false, 1, 1000))
            .ReturnsAsync(notifications);

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        await _service.SendDigestNotificationAsync(userId, DigestType.Daily);

        // Assert
        _mockNotificationRepository.Verify(r => r.CreateAsync(
            It.Is<Notification>(n => 
                n.UserId == userId &&
                n.Type == NotificationType.System &&
                n.Title == "Podsumowanie dnia" &&
                n.RelatedEntityType == "Digest")), Times.Once);

        _mockEmailService.Verify(e => e.SendNotificationEmailAsync(
            user.Email,
            user.FullName,
            It.IsAny<string>(),
            It.IsAny<string>(),
            "/dashboard/notifications",
            "Zobacz wszystkie powiadomienia"), Times.Once);
    }

    [Fact]
    public async Task SendDigestNotificationAsync_WeeklyDigest_CreatesWeeklyDigestNotification()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferences = new NotificationPreferences
        {
            UserId = userId,
            DigestEnabled = true,
            EmailEnabled = false // Email disabled
        };

        var notifications = new List<Notification>
        {
            new Notification
            {
                Id = Guid.NewGuid(),
                Type = NotificationType.RequestPendingApproval,
                Title = "Request 1",
                CreatedAt = DateTime.UtcNow.Date.AddDays(-3)
            }
        };

        _mockPreferencesRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(preferences);

        _mockNotificationRepository
            .Setup(r => r.GetUserNotificationsAsync(userId, false, 1, 1000))
            .ReturnsAsync(notifications);

        // Act
        await _service.SendDigestNotificationAsync(userId, DigestType.Weekly);

        // Assert
        _mockNotificationRepository.Verify(r => r.CreateAsync(
            It.Is<Notification>(n => 
                n.UserId == userId &&
                n.Type == NotificationType.System &&
                n.Title == "Podsumowanie tygodnia" &&
                n.RelatedEntityType == "Digest")), Times.Once);

        // Should not send email since email is disabled
        _mockEmailService.Verify(e => e.SendNotificationEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SendDigestNotificationAsync_NoNotificationsInPeriod_DoesNotCreateDigest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferences = new NotificationPreferences
        {
            UserId = userId,
            DigestEnabled = true
        };

        _mockPreferencesRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(preferences);

        _mockNotificationRepository
            .Setup(r => r.GetUserNotificationsAsync(userId, false, 1, 1000))
            .ReturnsAsync(new List<Notification>()); // No notifications

        // Act
        await _service.SendDigestNotificationAsync(userId, DigestType.Daily);

        // Assert
        _mockNotificationRepository.Verify(r => r.CreateAsync(It.IsAny<Notification>()), Times.Never);
        _mockEmailService.Verify(e => e.SendNotificationEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GenerateDigestAsync_NotificationsInPeriod_GeneratesCorrectDigest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var periodStart = DateTime.UtcNow.Date.AddDays(-1);
        var periodEnd = DateTime.UtcNow.Date;

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
                Type = NotificationType.RequestPendingApproval,
                Title = "Request 1",
                CreatedAt = periodStart.AddHours(2)
            },
            new Notification
            {
                Id = Guid.NewGuid(),
                Type = NotificationType.RequestPendingApproval,
                Title = "Request 2",
                CreatedAt = periodStart.AddHours(3)
            },
            new Notification
            {
                Id = Guid.NewGuid(),
                Type = NotificationType.RequestApproved,
                Title = "Request approved",
                CreatedAt = periodStart.AddHours(4)
            },
            new Notification
            {
                Id = Guid.NewGuid(),
                Type = NotificationType.System,
                Title = "System notification",
                CreatedAt = periodStart.AddDays(-2) // Outside period
            }
        };

        _mockPreferencesRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(preferences);

        _mockNotificationRepository
            .Setup(r => r.GetUserNotificationsAsync(userId, false, 1, 1000))
            .ReturnsAsync(notifications);

        // Act
        var result = await _service.GenerateDigestAsync(userId, periodStart, periodEnd);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(DigestType.Daily);
        result.PeriodStart.Should().Be(periodStart);
        result.PeriodEnd.Should().Be(periodEnd);
        result.TotalNotifications.Should().Be(3); // Only notifications within period
        result.Groups.Should().HaveCount(2); // Two different types
        result.Summary.Should().Contain("3 powiadomień");
        result.Summary.Should().Contain("2 kategoriach");
    }

    [Fact]
    public async Task GenerateDigestAsync_WeeklyPeriod_SetsCorrectDigestType()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var periodStart = DateTime.UtcNow.Date.AddDays(-7);
        var periodEnd = DateTime.UtcNow.Date;

        var preferences = new NotificationPreferences
        {
            UserId = userId,
            GroupSimilarNotifications = true
        };

        var notifications = new List<Notification>
        {
            new Notification
            {
                Id = Guid.NewGuid(),
                Type = NotificationType.RequestPendingApproval,
                Title = "Request 1",
                CreatedAt = periodStart.AddDays(1)
            }
        };

        _mockPreferencesRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(preferences);

        _mockNotificationRepository
            .Setup(r => r.GetUserNotificationsAsync(userId, false, 1, 1000))
            .ReturnsAsync(notifications);

        // Act
        var result = await _service.GenerateDigestAsync(userId, periodStart, periodEnd);

        // Assert
        result.Type.Should().Be(DigestType.Weekly);
    }

    [Fact]
    public async Task GenerateDigestAsync_SingleNotificationCategory_GeneratesSimpleSummary()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var periodStart = DateTime.UtcNow.Date.AddDays(-1);
        var periodEnd = DateTime.UtcNow.Date;

        var preferences = new NotificationPreferences
        {
            UserId = userId,
            GroupSimilarNotifications = true
        };

        var notifications = new List<Notification>
        {
            new Notification
            {
                Id = Guid.NewGuid(),
                Type = NotificationType.RequestApproved,
                Title = "Request approved",
                CreatedAt = periodStart.AddHours(2)
            }
        };

        _mockPreferencesRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(preferences);

        _mockNotificationRepository
            .Setup(r => r.GetUserNotificationsAsync(userId, false, 1, 1000))
            .ReturnsAsync(notifications);

        // Act
        var result = await _service.GenerateDigestAsync(userId, periodStart, periodEnd);

        // Assert
        result.TotalNotifications.Should().Be(1);
        result.Groups.Should().HaveCount(1);
        result.Summary.Should().Be("Masz 1 powiadomień. Najczęstsze: 1 zatwierdzone wnioski.");
    }

    [Fact]
    public async Task GenerateDigestAsync_MultipleCategories_IncludesTopThreeInSummary()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var periodStart = DateTime.UtcNow.Date.AddDays(-1);
        var periodEnd = DateTime.UtcNow.Date;

        var preferences = new NotificationPreferences
        {
            UserId = userId,
            GroupSimilarNotifications = true,
            MaxGroupSize = 10,
            GroupingTimeWindowMinutes = 120
        };

        var notifications = new List<Notification>();
        
        // Add 5 pending approval notifications
        for (int i = 0; i < 5; i++)
        {
            notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                Type = NotificationType.RequestPendingApproval,
                Title = $"Pending {i}",
                CreatedAt = periodStart.AddHours(i)
            });
        }
        
        // Add 3 approved notifications
        for (int i = 0; i < 3; i++)
        {
            notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                Type = NotificationType.RequestApproved,
                Title = $"Approved {i}",
                CreatedAt = periodStart.AddHours(i + 5)
            });
        }
        
        // Add 2 system notifications
        for (int i = 0; i < 2; i++)
        {
            notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                Type = NotificationType.System,
                Title = $"System {i}",
                CreatedAt = periodStart.AddHours(i + 8)
            });
        }
        
        // Add 1 rejected notification
        notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            Type = NotificationType.RequestRejected,
            Title = "Rejected",
            CreatedAt = periodStart.AddHours(10)
        });

        _mockPreferencesRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(preferences);

        _mockNotificationRepository
            .Setup(r => r.GetUserNotificationsAsync(userId, false, 1, 1000))
            .ReturnsAsync(notifications);

        // Act
        var result = await _service.GenerateDigestAsync(userId, periodStart, periodEnd);

        // Assert
        result.TotalNotifications.Should().Be(11);
        result.Groups.Should().HaveCount(4);
        result.Summary.Should().Contain("11 powiadomień");
        result.Summary.Should().Contain("4 kategoriach");
        result.Summary.Should().Contain("5 oczekujące zatwierdzenie");
        result.Summary.Should().Contain("3 zatwierdzone wnioski");
        result.Summary.Should().Contain("2 systemowe");
        // Should only show top 3, so rejected should not be in summary
        result.Summary.Should().NotContain("odrzucone");
    }

    [Fact]
    public async Task SendDigestNotificationAsync_EmailServiceThrows_ContinuesWithoutError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferences = new NotificationPreferences
        {
            UserId = userId,
            DigestEnabled = true,
            EmailEnabled = true
        };

        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var notifications = new List<Notification>
        {
            new Notification
            {
                Id = Guid.NewGuid(),
                Type = NotificationType.RequestPendingApproval,
                Title = "Request 1",
                CreatedAt = DateTime.UtcNow.Date.AddHours(-2)
            }
        };

        _mockPreferencesRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(preferences);

        _mockNotificationRepository
            .Setup(r => r.GetUserNotificationsAsync(userId, false, 1, 1000))
            .ReturnsAsync(notifications);

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _mockEmailService
            .Setup(e => e.SendNotificationEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ThrowsAsync(new Exception("Email service error"));

        // Act & Assert
        // Should not throw exception
        await _service.SendDigestNotificationAsync(userId, DigestType.Daily);

        // Notification should still be created despite email failure
        _mockNotificationRepository.Verify(r => r.CreateAsync(
            It.Is<Notification>(n => 
                n.UserId == userId &&
                n.Type == NotificationType.System)), Times.Once);
    }
}