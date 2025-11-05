using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Tests.Unit.Application;

public class ServiceRequestHandlerTests
{
    private readonly Mock<IRequestRepository> _mockRequestRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<IServiceCategoryConfigService> _mockServiceCategoryConfigService;
    private readonly Mock<IRequestRoutingService> _mockRequestRoutingService;
    private readonly Mock<ILogger<ServiceRequestHandler>> _mockLogger;
    private readonly ServiceRequestHandler _handler;

    public ServiceRequestHandlerTests()
    {
        _mockRequestRepository = new Mock<IRequestRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockServiceCategoryConfigService = new Mock<IServiceCategoryConfigService>();
        _mockRequestRoutingService = new Mock<IRequestRoutingService>();
        _mockLogger = new Mock<ILogger<ServiceRequestHandler>>();

        _handler = new ServiceRequestHandler(
            _mockRequestRepository.Object,
            _mockUserRepository.Object,
            _mockNotificationService.Object,
            _mockServiceCategoryConfigService.Object,
            _mockRequestRoutingService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ProcessServiceRequestAsync_ValidServiceRequest_ProcessesSuccessfully()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var submitterId = Guid.NewGuid();
        var serviceCategory = "IT";

        var submittedBy = new User
        {
            Id = submitterId,
            FirstName = "John",
            LastName = "Doe"
        };

        var request = new Request
        {
            Id = requestId,
            RequestNumber = "REQ-001",
            ServiceCategory = serviceCategory,
            SubmittedById = submitterId,
            SubmittedBy = submittedBy
        };

        var serviceCategoryConfig = new ServiceCategoryConfig
        {
            CategoryName = serviceCategory,
            DisplayName = "Information Technology",
            IsActive = true
        };

        _mockServiceCategoryConfigService
            .Setup(s => s.GetServiceCategoryConfigAsync(serviceCategory))
            .ReturnsAsync(serviceCategoryConfig);

        _mockServiceCategoryConfigService
            .Setup(s => s.GetRoutingRulesAsync(serviceCategory))
            .ReturnsAsync(new List<ServiceRoutingRule>());

        // Act
        var result = await _handler.ProcessServiceRequestAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.AssignedTeam.Should().Be("Information Technology");
        result.ErrorMessage.Should().BeNull();

        _mockRequestRepository.Verify(r => r.UpdateAsync(
            It.Is<Request>(req => req.ServiceStatus == ServiceTaskStatus.Assigned)),
            Times.Once);
    }

    [Fact]
    public async Task ProcessServiceRequestAsync_NoServiceCategory_ReturnsError()
    {
        // Arrange
        var request = new Request
        {
            Id = Guid.NewGuid(),
            RequestNumber = "REQ-002",
            ServiceCategory = null // No service category
        };

        // Act
        var result = await _handler.ProcessServiceRequestAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Service category is required for service requests");

        _mockRequestRepository.Verify(r => r.UpdateAsync(It.IsAny<Request>()), Times.Never);
    }

    [Fact]
    public async Task ProcessServiceRequestAsync_UnknownServiceCategory_ReturnsError()
    {
        // Arrange
        var request = new Request
        {
            Id = Guid.NewGuid(),
            RequestNumber = "REQ-003",
            ServiceCategory = "UnknownCategory"
        };

        _mockServiceCategoryConfigService
            .Setup(s => s.GetServiceCategoryConfigAsync("UnknownCategory"))
            .ReturnsAsync((ServiceCategoryConfig?)null);

        // Act
        var result = await _handler.ProcessServiceRequestAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Unknown service category: UnknownCategory");
    }

    [Fact]
    public async Task CanHandleRequestTypeAsync_ValidServiceCategory_ReturnsTrue()
    {
        // Arrange
        var serviceCategory = "HR";
        var serviceCategoryConfig = new ServiceCategoryConfig
        {
            CategoryName = serviceCategory,
            IsActive = true
        };

        _mockServiceCategoryConfigService
            .Setup(s => s.GetServiceCategoryConfigAsync(serviceCategory))
            .ReturnsAsync(serviceCategoryConfig);

        // Act
        var result = await _handler.CanHandleRequestTypeAsync(serviceCategory);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanHandleRequestTypeAsync_InactiveServiceCategory_ReturnsFalse()
    {
        // Arrange
        var serviceCategory = "Facilities";
        var serviceCategoryConfig = new ServiceCategoryConfig
        {
            CategoryName = serviceCategory,
            IsActive = false // Inactive
        };

        _mockServiceCategoryConfigService
            .Setup(s => s.GetServiceCategoryConfigAsync(serviceCategory))
            .ReturnsAsync(serviceCategoryConfig);

        // Act
        var result = await _handler.CanHandleRequestTypeAsync(serviceCategory);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanHandleRequestTypeAsync_NonExistentServiceCategory_ReturnsFalse()
    {
        // Arrange
        var serviceCategory = "NonExistent";

        _mockServiceCategoryConfigService
            .Setup(s => s.GetServiceCategoryConfigAsync(serviceCategory))
            .ReturnsAsync((ServiceCategoryConfig?)null);

        // Act
        var result = await _handler.CanHandleRequestTypeAsync(serviceCategory);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task NotifyServiceTeamAsync_ValidServiceCategory_LogsNotification()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var serviceCategory = "Finance";
        var request = new Request
        {
            Id = requestId,
            RequestNumber = "REQ-004",
            ServiceCategory = serviceCategory,
            SubmittedBy = new User
            {
                FirstName = "Jane",
                LastName = "Smith"
            }
        };

        var serviceCategoryConfig = new ServiceCategoryConfig
        {
            CategoryName = serviceCategory,
            DisplayName = "Finance Department",
            IsActive = true
        };

        var routingRules = new List<ServiceRoutingRule>
        {
            new ServiceRoutingRule
            {
                RuleName = "Budget Requests",
                TargetRoles = new List<string> { "FinanceManager" },
                Priority = 1
            }
        };

        _mockServiceCategoryConfigService
            .Setup(s => s.GetServiceCategoryConfigAsync(serviceCategory))
            .ReturnsAsync(serviceCategoryConfig);

        _mockServiceCategoryConfigService
            .Setup(s => s.GetRoutingRulesAsync(serviceCategory))
            .ReturnsAsync(routingRules);

        // Act
        await _handler.NotifyServiceTeamAsync(request, serviceCategory);

        // Assert
        // Verify that the method completes without throwing
        // In a real implementation, this would verify actual notifications were sent
        _mockServiceCategoryConfigService.Verify(
            s => s.GetServiceCategoryConfigAsync(serviceCategory),
            Times.Once);
        _mockServiceCategoryConfigService.Verify(
            s => s.GetRoutingRulesAsync(serviceCategory),
            Times.Once);
    }

    [Fact]
    public async Task NotifyServiceTeamAsync_UnknownServiceCategory_LogsWarning()
    {
        // Arrange
        var request = new Request
        {
            Id = Guid.NewGuid(),
            RequestNumber = "REQ-005",
            ServiceCategory = "Unknown"
        };

        _mockServiceCategoryConfigService
            .Setup(s => s.GetServiceCategoryConfigAsync("Unknown"))
            .ReturnsAsync((ServiceCategoryConfig?)null);

        // Act
        await _handler.NotifyServiceTeamAsync(request, "Unknown");

        // Assert
        // Method should complete without throwing, but log a warning
        _mockServiceCategoryConfigService.Verify(
            s => s.GetServiceCategoryConfigAsync("Unknown"),
            Times.Once);
        _mockServiceCategoryConfigService.Verify(
            s => s.GetRoutingRulesAsync(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateServiceTaskStatusAsync_ValidRequest_UpdatesStatusSuccessfully()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var submitterId = Guid.NewGuid();
        var status = ServiceTaskStatus.InProgress;
        var notes = "Started working on the request";

        var request = new Request
        {
            Id = requestId,
            ServiceCategory = "IT",
            SubmittedById = submitterId,
            Status = RequestStatus.InReview
        };

        _mockRequestRepository
            .Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(request);

        // Act
        await _handler.UpdateServiceTaskStatusAsync(requestId, status, notes);

        // Assert
        _mockRequestRepository.Verify(r => r.UpdateAsync(
            It.Is<Request>(req => 
                req.ServiceStatus == status &&
                req.ServiceNotes.Contains(notes))),
            Times.Once);

        _mockNotificationService.Verify(n => n.NotifySubmitterAsync(
            It.IsAny<Request>(),
            It.Is<string>(msg => msg.Contains("being processed")),
            NotificationType.RequestCompleted),
            Times.Once);
    }

    [Fact]
    public async Task UpdateServiceTaskStatusAsync_CompletedStatus_UpdatesRequestStatus()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var submitterId = Guid.NewGuid();
        var status = ServiceTaskStatus.Completed;
        var notes = "Task completed successfully";

        var request = new Request
        {
            Id = requestId,
            ServiceCategory = "HR",
            SubmittedById = submitterId,
            Status = RequestStatus.InReview
        };

        _mockRequestRepository
            .Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(request);

        // Act
        await _handler.UpdateServiceTaskStatusAsync(requestId, status, notes);

        // Assert
        _mockRequestRepository.Verify(r => r.UpdateAsync(
            It.Is<Request>(req => 
                req.ServiceStatus == ServiceTaskStatus.Completed &&
                req.Status == RequestStatus.Approved &&
                req.CompletedAt.HasValue &&
                req.ServiceCompletedAt.HasValue)),
            Times.Once);

        _mockNotificationService.Verify(n => n.NotifySubmitterAsync(
            It.IsAny<Request>(),
            It.Is<string>(msg => msg.Contains("completed")),
            NotificationType.RequestCompleted),
            Times.Once);
    }

    [Fact]
    public async Task UpdateServiceTaskStatusAsync_RequestNotFound_ThrowsException()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var status = ServiceTaskStatus.InProgress;
        var notes = "Some notes";

        _mockRequestRepository
            .Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync((Request?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.UpdateServiceTaskStatusAsync(requestId, status, notes));

        exception.Message.Should().Contain($"Request with ID {requestId} not found");
    }

    [Fact]
    public async Task UpdateServiceTaskStatusAsync_NonServiceRequest_ThrowsException()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var status = ServiceTaskStatus.InProgress;
        var notes = "Some notes";

        var request = new Request
        {
            Id = requestId,
            ServiceCategory = null // Not a service request
        };

        _mockRequestRepository
            .Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(request);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.UpdateServiceTaskStatusAsync(requestId, status, notes));

        exception.Message.Should().Contain($"Request {requestId} is not a service request");
    }

    [Fact]
    public async Task UpdateServiceTaskStatusAsync_OnHoldStatus_UpdatesWithNotes()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var status = ServiceTaskStatus.OnHold;
        var notes = "Waiting for additional information";

        var request = new Request
        {
            Id = requestId,
            ServiceCategory = "Legal",
            ServiceNotes = "Previous note"
        };

        _mockRequestRepository
            .Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(request);

        // Act
        await _handler.UpdateServiceTaskStatusAsync(requestId, status, notes);

        // Assert
        _mockRequestRepository.Verify(r => r.UpdateAsync(
            It.Is<Request>(req => 
                req.ServiceStatus == ServiceTaskStatus.OnHold &&
                req.ServiceNotes.Contains("Previous note") &&
                req.ServiceNotes.Contains(notes))),
            Times.Once);

        _mockNotificationService.Verify(n => n.NotifySubmitterAsync(
            It.IsAny<Request>(),
            It.Is<string>(msg => msg.Contains("on hold")),
            NotificationType.RequestCompleted),
            Times.Once);
    }
}