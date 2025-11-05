using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.Requests.Commands.CloneRequest;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Tests.Unit.Requests;

public class CloneRequestCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRequestRepository> _mockRequestRepo;
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<ILogger<CloneRequestCommandHandler>> _mockLogger;
    private readonly CloneRequestCommandHandler _handler;

    public CloneRequestCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockRequestRepo = new Mock<IRequestRepository>();
        _mockUserRepo = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<CloneRequestCommandHandler>>();

        _mockUnitOfWork.Setup(u => u.RequestRepository).Returns(_mockRequestRepo.Object);
        _mockUnitOfWork.Setup(u => u.UserRepository).Returns(_mockUserRepo.Object);

        _handler = new CloneRequestCommandHandler(
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidCloneRequest_ClonesRequestSuccessfully()
    {
        // Arrange
        var originalRequestId = Guid.NewGuid();
        var clonedById = Guid.NewGuid();
        var templateId = Guid.NewGuid();

        var originalRequest = new Request
        {
            Id = originalRequestId,
            RequestNumber = "REQ-2024-0001",
            RequestTemplateId = templateId,
            SubmittedById = clonedById,
            SubmittedAt = DateTime.UtcNow.AddDays(-30),
            Priority = RequestPriority.Urgent,
            FormData = "{\"equipment\":\"Laptop\",\"reason\":\"Development work\"}",
            Status = RequestStatus.Approved,
            LeaveType = null,
            ServiceCategory = "IT",
            Tags = "[\"urgent\",\"development\"]"
        };

        var clonedByUser = new User
        {
            Id = clonedById,
            FirstName = "Jane",
            LastName = "Employee",
            Role = UserRole.Employee
        };

        _mockRequestRepo.Setup(r => r.GetByIdAsync(originalRequestId))
            .ReturnsAsync(originalRequest);
        _mockUserRepo.Setup(r => r.GetByIdAsync(clonedById))
            .ReturnsAsync(clonedByUser);
        _mockRequestRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Request> { originalRequest });

        var command = new CloneRequestCommand
        {
            OriginalRequestId = originalRequestId,
            ClonedById = clonedById,
            CreateAsTemplate = false
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.RequestNumber.Should().StartWith("REQ-");
        result.Message.Should().Be("Request cloned successfully");
        result.IsTemplate.Should().BeFalse();

        _mockRequestRepo.Verify(r => r.CreateAsync(
            It.Is<Request>(req =>
                req.ClonedFromId == originalRequestId &&
                req.SubmittedById == clonedById &&
                req.FormData == originalRequest.FormData &&
                req.Status == RequestStatus.Draft &&
                req.IsTemplate == false &&
                req.Attachments == null // Should be cleared
            )),
            Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_CreateAsTemplate_CreatesTemplateSuccessfully()
    {
        // Arrange
        var originalRequestId = Guid.NewGuid();
        var clonedById = Guid.NewGuid();

        var originalRequest = new Request
        {
            Id = originalRequestId,
            RequestTemplateId = Guid.NewGuid(),
            SubmittedById = clonedById,
            FormData = "{\"equipment\":\"Standard Laptop\"}",
            Status = RequestStatus.Approved
        };

        var clonedByUser = new User
        {
            Id = clonedById,
            Role = UserRole.Employee
        };

        _mockRequestRepo.Setup(r => r.GetByIdAsync(originalRequestId))
            .ReturnsAsync(originalRequest);
        _mockUserRepo.Setup(r => r.GetByIdAsync(clonedById))
            .ReturnsAsync(clonedByUser);
        _mockRequestRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Request>());

        var command = new CloneRequestCommand
        {
            OriginalRequestId = originalRequestId,
            ClonedById = clonedById,
            CreateAsTemplate = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.RequestNumber.Should().StartWith("TMPL-");
        result.Message.Should().Be("Request template created successfully");
        result.IsTemplate.Should().BeTrue();

        _mockRequestRepo.Verify(r => r.CreateAsync(
            It.Is<Request>(req => req.IsTemplate == true)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ModifiedFormData_UsesModifiedData()
    {
        // Arrange
        var originalRequestId = Guid.NewGuid();
        var clonedById = Guid.NewGuid();

        var originalRequest = new Request
        {
            Id = originalRequestId,
            RequestTemplateId = Guid.NewGuid(),
            SubmittedById = clonedById,
            FormData = "{\"equipment\":\"Old Laptop\"}",
            Status = RequestStatus.Approved
        };

        var clonedByUser = new User
        {
            Id = clonedById,
            Role = UserRole.Employee
        };

        var modifiedFormData = "{\"equipment\":\"New Desktop\",\"reason\":\"Updated requirements\"}";

        _mockRequestRepo.Setup(r => r.GetByIdAsync(originalRequestId))
            .ReturnsAsync(originalRequest);
        _mockUserRepo.Setup(r => r.GetByIdAsync(clonedById))
            .ReturnsAsync(clonedByUser);
        _mockRequestRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Request>());

        var command = new CloneRequestCommand
        {
            OriginalRequestId = originalRequestId,
            ClonedById = clonedById,
            ModifiedFormData = modifiedFormData
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockRequestRepo.Verify(r => r.CreateAsync(
            It.Is<Request>(req => req.FormData == modifiedFormData)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_OriginalRequestNotFound_ThrowsException()
    {
        // Arrange
        var originalRequestId = Guid.NewGuid();
        var clonedById = Guid.NewGuid();

        _mockRequestRepo.Setup(r => r.GetByIdAsync(originalRequestId))
            .ReturnsAsync((Request?)null);

        var command = new CloneRequestCommand
        {
            OriginalRequestId = originalRequestId,
            ClonedById = clonedById
        };

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsException()
    {
        // Arrange
        var originalRequestId = Guid.NewGuid();
        var clonedById = Guid.NewGuid();

        var originalRequest = new Request
        {
            Id = originalRequestId,
            SubmittedById = Guid.NewGuid()
        };

        _mockRequestRepo.Setup(r => r.GetByIdAsync(originalRequestId))
            .ReturnsAsync(originalRequest);
        _mockUserRepo.Setup(r => r.GetByIdAsync(clonedById))
            .ReturnsAsync((User?)null);

        var command = new CloneRequestCommand
        {
            OriginalRequestId = originalRequestId,
            ClonedById = clonedById
        };

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var originalRequestId = Guid.NewGuid();
        var originalSubmitterId = Guid.NewGuid();
        var unauthorizedUserId = Guid.NewGuid();

        var originalRequest = new Request
        {
            Id = originalRequestId,
            SubmittedById = originalSubmitterId,
            LeaveType = LeaveType.Sick // Sensitive data
        };

        var unauthorizedUser = new User
        {
            Id = unauthorizedUserId,
            Role = UserRole.Employee
        };

        _mockRequestRepo.Setup(r => r.GetByIdAsync(originalRequestId))
            .ReturnsAsync(originalRequest);
        _mockUserRepo.Setup(r => r.GetByIdAsync(unauthorizedUserId))
            .ReturnsAsync(unauthorizedUser);
        _mockUserRepo.Setup(r => r.GetByIdAsync(originalSubmitterId))
            .ReturnsAsync(new User { Id = originalSubmitterId, SupervisorId = Guid.NewGuid() });

        var command = new CloneRequestCommand
        {
            OriginalRequestId = originalRequestId,
            ClonedById = unauthorizedUserId
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AdminUser_CanCloneAnyRequest()
    {
        // Arrange
        var originalRequestId = Guid.NewGuid();
        var originalSubmitterId = Guid.NewGuid();
        var adminUserId = Guid.NewGuid();

        var originalRequest = new Request
        {
            Id = originalRequestId,
            RequestTemplateId = Guid.NewGuid(),
            SubmittedById = originalSubmitterId,
            FormData = "{}",
            Status = RequestStatus.Approved
        };

        var adminUser = new User
        {
            Id = adminUserId,
            Role = UserRole.Admin // Admin can clone any request
        };

        _mockRequestRepo.Setup(r => r.GetByIdAsync(originalRequestId))
            .ReturnsAsync(originalRequest);
        _mockUserRepo.Setup(r => r.GetByIdAsync(adminUserId))
            .ReturnsAsync(adminUser);
        _mockRequestRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Request>());

        var command = new CloneRequestCommand
        {
            OriginalRequestId = originalRequestId,
            ClonedById = adminUserId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mockRequestRepo.Verify(r => r.CreateAsync(It.IsAny<Request>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidModifiedFormData_ThrowsException()
    {
        // Arrange
        var originalRequestId = Guid.NewGuid();
        var clonedById = Guid.NewGuid();

        var originalRequest = new Request
        {
            Id = originalRequestId,
            RequestTemplateId = Guid.NewGuid(),
            SubmittedById = clonedById,
            FormData = "{}",
            Status = RequestStatus.Approved
        };

        var clonedByUser = new User
        {
            Id = clonedById,
            Role = UserRole.Employee
        };

        _mockRequestRepo.Setup(r => r.GetByIdAsync(originalRequestId))
            .ReturnsAsync(originalRequest);
        _mockUserRepo.Setup(r => r.GetByIdAsync(clonedById))
            .ReturnsAsync(clonedByUser);

        var command = new CloneRequestCommand
        {
            OriginalRequestId = originalRequestId,
            ClonedById = clonedById,
            ModifiedFormData = "invalid json" // Invalid JSON
        };

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}