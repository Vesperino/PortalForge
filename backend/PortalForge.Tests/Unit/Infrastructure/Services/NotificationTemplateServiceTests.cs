using FluentAssertions;
using Moq;
using Xunit;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;
using PortalForge.Infrastructure.Services;

namespace PortalForge.Tests.Unit.Infrastructure.Services;

public class NotificationTemplateServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<INotificationTemplateRepository> _mockTemplateRepository;
    private readonly NotificationTemplateService _service;

    public NotificationTemplateServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockTemplateRepository = new Mock<INotificationTemplateRepository>();

        _mockUnitOfWork.Setup(u => u.NotificationTemplateRepository)
            .Returns(_mockTemplateRepository.Object);

        _service = new NotificationTemplateService(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task RenderNotificationAsync_ExistingTemplate_RendersWithPlaceholders()
    {
        // Arrange
        var type = NotificationType.RequestPendingApproval;
        var template = new NotificationTemplate
        {
            Id = Guid.NewGuid(),
            Type = type,
            TitleTemplate = "New request: {RequestType}",
            MessageTemplate = "{SubmitterName} submitted {RequestType} for approval.",
            Language = "pl"
        };

        var placeholders = new Dictionary<string, object>
        {
            { "RequestType", "Vacation Request" },
            { "SubmitterName", "John Doe" }
        };

        _mockTemplateRepository
            .Setup(r => r.GetByTypeAndLanguageAsync(type, "pl"))
            .ReturnsAsync(template);

        // Act
        var (title, message) = await _service.RenderNotificationAsync(type, placeholders, "pl");

        // Assert
        title.Should().Be("New request: Vacation Request");
        message.Should().Be("John Doe submitted Vacation Request for approval.");
    }

    [Fact]
    public async Task RenderNotificationAsync_NoExistingTemplate_CreatesDefaultAndRenders()
    {
        // Arrange
        var type = NotificationType.RequestPendingApproval;
        var placeholders = new Dictionary<string, object>
        {
            { "RequestType", "Vacation Request" },
            { "SubmitterName", "John Doe" }
        };

        _mockTemplateRepository
            .Setup(r => r.GetByTypeAndLanguageAsync(type, "pl"))
            .ReturnsAsync((NotificationTemplate?)null);

        // Act
        var (title, message) = await _service.RenderNotificationAsync(type, placeholders, "pl");

        // Assert
        title.Should().Contain("Vacation Request");
        message.Should().Contain("John Doe");
        
        _mockTemplateRepository.Verify(r => r.CreateAsync(
            It.Is<NotificationTemplate>(t => 
                t.Type == type && 
                t.Language == "pl")), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RenderEmailNotificationAsync_TemplateWithEmailFields_RendersEmailContent()
    {
        // Arrange
        var type = NotificationType.RequestApproved;
        var template = new NotificationTemplate
        {
            Id = Guid.NewGuid(),
            Type = type,
            TitleTemplate = "Request approved: {RequestType}",
            MessageTemplate = "Your {RequestType} has been approved by {ApproverName}.",
            EmailSubjectTemplate = "✅ {RequestType} - Approved",
            EmailBodyTemplate = "Hello {SubmitterName},\n\nYour {RequestType} has been approved by {ApproverName}.\n\nBest regards,\nPortalForge Team",
            Language = "pl"
        };

        var placeholders = new Dictionary<string, object>
        {
            { "RequestType", "Vacation Request" },
            { "SubmitterName", "Jane Smith" },
            { "ApproverName", "John Manager" }
        };

        _mockTemplateRepository
            .Setup(r => r.GetByTypeAndLanguageAsync(type, "pl"))
            .ReturnsAsync(template);

        // Act
        var (subject, body) = await _service.RenderEmailNotificationAsync(type, placeholders, "pl");

        // Assert
        subject.Should().Be("✅ Vacation Request - Approved");
        body.Should().Contain("Hello Jane Smith");
        body.Should().Contain("approved by John Manager");
        body.Should().Contain("PortalForge Team");
    }

    [Fact]
    public async Task RenderEmailNotificationAsync_TemplateWithoutEmailFields_FallsBackToTitleAndMessage()
    {
        // Arrange
        var type = NotificationType.RequestRejected;
        var template = new NotificationTemplate
        {
            Id = Guid.NewGuid(),
            Type = type,
            TitleTemplate = "Request rejected: {RequestType}",
            MessageTemplate = "Your {RequestType} has been rejected. Reason: {Reason}",
            EmailSubjectTemplate = null, // No email-specific template
            EmailBodyTemplate = null,
            Language = "pl"
        };

        var placeholders = new Dictionary<string, object>
        {
            { "RequestType", "Vacation Request" },
            { "Reason", "Insufficient vacation days" }
        };

        _mockTemplateRepository
            .Setup(r => r.GetByTypeAndLanguageAsync(type, "pl"))
            .ReturnsAsync(template);

        // Act
        var (subject, body) = await _service.RenderEmailNotificationAsync(type, placeholders, "pl");

        // Assert
        subject.Should().Be("Request rejected: Vacation Request");
        body.Should().Be("Your Vacation Request has been rejected. Reason: Insufficient vacation days");
    }

    [Fact]
    public async Task GetOrCreateDefaultTemplateAsync_ExistingTemplate_ReturnsExisting()
    {
        // Arrange
        var type = NotificationType.System;
        var existingTemplate = new NotificationTemplate
        {
            Id = Guid.NewGuid(),
            Type = type,
            Name = "System Notification",
            Language = "pl"
        };

        _mockTemplateRepository
            .Setup(r => r.GetByTypeAndLanguageAsync(type, "pl"))
            .ReturnsAsync(existingTemplate);

        // Act
        var result = await _service.GetOrCreateDefaultTemplateAsync(type, "pl");

        // Assert
        result.Should().Be(existingTemplate);
        _mockTemplateRepository.Verify(r => r.CreateAsync(It.IsAny<NotificationTemplate>()), Times.Never);
    }

    [Fact]
    public async Task GetOrCreateDefaultTemplateAsync_NoExistingTemplate_CreatesDefault()
    {
        // Arrange
        var type = NotificationType.VacationCoverageAssigned;

        _mockTemplateRepository
            .Setup(r => r.GetByTypeAndLanguageAsync(type, "pl"))
            .ReturnsAsync((NotificationTemplate?)null);

        // Act
        var result = await _service.GetOrCreateDefaultTemplateAsync(type, "pl");

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(type);
        result.Language.Should().Be("pl");
        result.IsActive.Should().BeTrue();
        result.TitleTemplate.Should().NotBeEmpty();
        result.MessageTemplate.Should().NotBeEmpty();

        _mockTemplateRepository.Verify(r => r.CreateAsync(
            It.Is<NotificationTemplate>(t => t.Type == type)), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateDefaultTemplatesAsync_CreatesTemplatesForAllTypes()
    {
        // Arrange
        var notificationTypes = Enum.GetValues<NotificationType>();
        
        _mockTemplateRepository
            .Setup(r => r.ExistsAsync(It.IsAny<NotificationType>(), "pl"))
            .ReturnsAsync(false);

        // Act
        await _service.CreateDefaultTemplatesAsync("pl");

        // Assert
        _mockTemplateRepository.Verify(r => r.CreateAsync(It.IsAny<NotificationTemplate>()), 
            Times.Exactly(notificationTypes.Length));
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateDefaultTemplatesAsync_SkipsExistingTemplates()
    {
        // Arrange
        _mockTemplateRepository
            .Setup(r => r.ExistsAsync(NotificationType.RequestPendingApproval, "pl"))
            .ReturnsAsync(true); // This one exists

        _mockTemplateRepository
            .Setup(r => r.ExistsAsync(It.Is<NotificationType>(t => t != NotificationType.RequestPendingApproval), "pl"))
            .ReturnsAsync(false); // Others don't exist

        var totalTypes = Enum.GetValues<NotificationType>().Length;

        // Act
        await _service.CreateDefaultTemplatesAsync("pl");

        // Assert
        _mockTemplateRepository.Verify(r => r.CreateAsync(It.IsAny<NotificationTemplate>()), 
            Times.Exactly(totalTypes - 1)); // One less because one already exists
    }

    [Fact]
    public async Task ValidateTemplateAsync_AllPlaceholdersProvided_ReturnsNoErrors()
    {
        // Arrange
        var template = new NotificationTemplate
        {
            TitleTemplate = "Hello {UserName}",
            MessageTemplate = "Your {RequestType} is {Status}",
            EmailSubjectTemplate = "Update: {RequestType}",
            EmailBodyTemplate = "Dear {UserName}, your {RequestType} is now {Status}."
        };

        var placeholders = new Dictionary<string, object>
        {
            { "UserName", "John Doe" },
            { "RequestType", "Vacation Request" },
            { "Status", "Approved" }
        };

        // Act
        var errors = await _service.ValidateTemplateAsync(template, placeholders);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateTemplateAsync_MissingPlaceholders_ReturnsErrors()
    {
        // Arrange
        var template = new NotificationTemplate
        {
            TitleTemplate = "Hello {UserName}",
            MessageTemplate = "Your {RequestType} is {Status}",
            EmailSubjectTemplate = "Update: {RequestType}",
            EmailBodyTemplate = "Dear {UserName}, your {RequestType} needs {MissingField}."
        };

        var placeholders = new Dictionary<string, object>
        {
            { "UserName", "John Doe" },
            { "RequestType", "Vacation Request" }
            // Missing: Status, MissingField
        };

        // Act
        var errors = await _service.ValidateTemplateAsync(template, placeholders);

        // Assert
        errors.Should().HaveCount(2);
        errors.Should().Contain(e => e.Contains("Status"));
        errors.Should().Contain(e => e.Contains("MissingField"));
    }

    [Theory]
    [InlineData("Hello {UserName}, your request is ready", "UserName", "John Doe", "Hello John Doe, your request is ready")]
    [InlineData("Status: {Status} for {RequestType}", "Status,RequestType", "Approved,Vacation", "Status: Approved for Vacation")]
    [InlineData("No placeholders here", "", "", "No placeholders here")]
    [InlineData("{Missing} placeholder", "Other", "Value", "{Missing} placeholder")]
    public async Task ReplacePlaceholders_VariousScenarios_ReplacesCorrectly(
        string template, 
        string placeholderKeys, 
        string placeholderValues, 
        string expected)
    {
        // Arrange
        var placeholders = new Dictionary<string, object>();
        if (!string.IsNullOrEmpty(placeholderKeys))
        {
            var keys = placeholderKeys.Split(',');
            var values = placeholderValues.Split(',');
            for (int i = 0; i < keys.Length; i++)
            {
                placeholders[keys[i]] = values[i];
            }
        }

        var notificationTemplate = new NotificationTemplate
        {
            TitleTemplate = template,
            MessageTemplate = template,
            Type = NotificationType.System,
            Language = "pl"
        };

        _mockTemplateRepository
            .Setup(r => r.GetByTypeAndLanguageAsync(NotificationType.System, "pl"))
            .ReturnsAsync(notificationTemplate);

        // Act
        var result = await _service.RenderNotificationAsync(NotificationType.System, placeholders, "pl");
        var (title, message) = result;

        // Assert
        title.Should().Be(expected);
        message.Should().Be(expected);
    }

    [Fact]
    public async Task RenderNotificationAsync_PolishLanguage_UsesPolishTemplate()
    {
        // Arrange
        var type = NotificationType.RequestPendingApproval;
        var placeholders = new Dictionary<string, object>
        {
            { "RequestType", "Wniosek urlopowy" },
            { "SubmitterName", "Jan Kowalski" }
        };

        _mockTemplateRepository
            .Setup(r => r.GetByTypeAndLanguageAsync(type, "pl"))
            .ReturnsAsync((NotificationTemplate?)null);

        // Act
        var (title, message) = await _service.RenderNotificationAsync(type, placeholders, "pl");

        // Assert
        title.Should().Contain("Wniosek urlopowy");
        message.Should().Contain("Jan Kowalski");
        message.Should().Contain("zatwierdzenie"); // Polish word for approval
        
        _mockTemplateRepository.Verify(r => r.CreateAsync(
            It.Is<NotificationTemplate>(t => 
                t.Language == "pl" && 
                t.TitleTemplate.Contains("zatwierdzenie"))), Times.Once);
    }

    [Fact]
    public async Task RenderNotificationAsync_EnglishLanguage_UsesEnglishTemplate()
    {
        // Arrange
        var type = NotificationType.RequestPendingApproval;
        var placeholders = new Dictionary<string, object>
        {
            { "RequestType", "Vacation Request" },
            { "SubmitterName", "John Smith" }
        };

        _mockTemplateRepository
            .Setup(r => r.GetByTypeAndLanguageAsync(type, "en"))
            .ReturnsAsync((NotificationTemplate?)null);

        // Act
        var (title, message) = await _service.RenderNotificationAsync(type, placeholders, "en");

        // Assert
        title.Should().Contain("Vacation Request");
        message.Should().Contain("John Smith");
        message.Should().Contain("approval"); // English word
        
        _mockTemplateRepository.Verify(r => r.CreateAsync(
            It.Is<NotificationTemplate>(t => 
                t.Language == "en" && 
                t.TitleTemplate.Contains("approval"))), Times.Once);
    }
}