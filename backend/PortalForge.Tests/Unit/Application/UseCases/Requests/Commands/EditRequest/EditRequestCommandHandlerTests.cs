using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;
using PortalForge.Application.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Application.UseCases.Requests.Commands.EditRequest;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Tests.Unit.Application.UseCases.Requests.Commands.EditRequest;

public class EditRequestCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IUnifiedValidatorService> _validatorServiceMock;
    private readonly Mock<ILogger<EditRequestCommandHandler>> _loggerMock;
    private readonly Mock<IRequestEditHistoryRepository> _requestEditHistoryRepositoryMock;
    private readonly EditRequestCommandHandler _handler;

    public EditRequestCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _notificationServiceMock = new Mock<INotificationService>();
        _validatorServiceMock = new Mock<IUnifiedValidatorService>();
        _loggerMock = new Mock<ILogger<EditRequestCommandHandler>>();
        _requestEditHistoryRepositoryMock = new Mock<IRequestEditHistoryRepository>();

        // Setup RequestEditHistoryRepository in UnitOfWork
        _unitOfWorkMock.Setup(x => x.RequestEditHistoryRepository)
            .Returns(_requestEditHistoryRepositoryMock.Object);

        _handler = new EditRequestCommandHandler(
            _unitOfWorkMock.Object,
            _notificationServiceMock.Object,
            _validatorServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_RequestNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var command = new EditRequestCommand
        {
            RequestId = requestId,
            EditedByUserId = Guid.NewGuid(),
            NewFormData = "{\"field\":\"new value\"}",
            ChangeReason = "Update"
        };

        _validatorServiceMock.Setup(x => x.ValidateAsync(command))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.RequestRepository.GetByIdAsync(requestId))
            .ReturnsAsync((Request?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Request with ID {requestId} not found");
    }

    [Fact]
    public async Task Handle_NotSubmitter_ThrowsForbiddenException()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var submitterId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var existingRequest = new Request
        {
            Id = requestId,
            RequestNumber = "REQ-001",
            SubmittedById = submitterId,
            Status = RequestStatus.InReview,
            FormData = "{\"old\":\"data\"}",
            SubmittedAt = DateTime.UtcNow
        };

        var command = new EditRequestCommand
        {
            RequestId = requestId,
            EditedByUserId = otherUserId, // Different from submitter
            NewFormData = "{\"new\":\"data\"}",
            ChangeReason = "Update"
        };

        _validatorServiceMock.Setup(x => x.ValidateAsync(command))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.RequestRepository.GetByIdAsync(requestId))
            .ReturnsAsync(existingRequest);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Możesz edytować tylko własne wnioski");
    }

    [Theory]
    [InlineData(RequestStatus.Approved)]
    [InlineData(RequestStatus.Rejected)]
    [InlineData(RequestStatus.AwaitingSurvey)]
    public async Task Handle_InvalidStatus_ThrowsValidationException(RequestStatus status)
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var existingRequest = new Request
        {
            Id = requestId,
            RequestNumber = "REQ-001",
            SubmittedById = userId,
            Status = status,
            FormData = "{\"old\":\"data\"}",
            SubmittedAt = DateTime.UtcNow
        };

        var command = new EditRequestCommand
        {
            RequestId = requestId,
            EditedByUserId = userId,
            NewFormData = "{\"new\":\"data\"}",
            ChangeReason = "Update"
        };

        _validatorServiceMock.Setup(x => x.ValidateAsync(command))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.RequestRepository.GetByIdAsync(requestId))
            .ReturnsAsync(existingRequest);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Draft*InReview*");
    }

    [Fact]
    public async Task Handle_ValidDraftRequest_EditsSuccessfully()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var existingRequest = new Request
        {
            Id = requestId,
            RequestNumber = "REQ-001",
            SubmittedById = userId,
            Status = RequestStatus.Draft,
            FormData = "{\"old\":\"data\"}",
            SubmittedAt = DateTime.UtcNow,
            ApprovalSteps = new List<RequestApprovalStep>()
        };

        var command = new EditRequestCommand
        {
            RequestId = requestId,
            EditedByUserId = userId,
            NewFormData = "{\"new\":\"data\"}",
            ChangeReason = "Korekta danych"
        };

        _validatorServiceMock.Setup(x => x.ValidateAsync(command))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.RequestRepository.GetByIdAsync(requestId))
            .ReturnsAsync(existingRequest);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);
        existingRequest.FormData.Should().Be("{\"new\":\"data\"}");

        _unitOfWorkMock.Verify(x => x.RequestRepository.UpdateAsync(existingRequest), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidInReviewRequest_EditsSuccessfully()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var existingRequest = new Request
        {
            Id = requestId,
            RequestNumber = "REQ-002",
            SubmittedById = userId,
            Status = RequestStatus.InReview,
            FormData = "{\"old\":\"data\"}",
            SubmittedAt = DateTime.UtcNow,
            ApprovalSteps = new List<RequestApprovalStep>()
        };

        var command = new EditRequestCommand
        {
            RequestId = requestId,
            EditedByUserId = userId,
            NewFormData = "{\"new\":\"data\"}",
            ChangeReason = "Zmiana dat"
        };

        _validatorServiceMock.Setup(x => x.ValidateAsync(command))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.RequestRepository.GetByIdAsync(requestId))
            .ReturnsAsync(existingRequest);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);
        existingRequest.FormData.Should().Be("{\"new\":\"data\"}");
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesEditHistory()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var existingRequest = new Request
        {
            Id = requestId,
            RequestNumber = "REQ-003",
            SubmittedById = userId,
            Status = RequestStatus.Draft,
            FormData = "{\"start\":\"2025-01-15\",\"end\":\"2025-01-20\"}",
            SubmittedAt = DateTime.UtcNow,
            ApprovalSteps = new List<RequestApprovalStep>()
        };

        var command = new EditRequestCommand
        {
            RequestId = requestId,
            EditedByUserId = userId,
            NewFormData = "{\"start\":\"2025-01-16\",\"end\":\"2025-01-21\"}",
            ChangeReason = "Zmiana terminu o 1 dzień"
        };

        _validatorServiceMock.Setup(x => x.ValidateAsync(command))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.RequestRepository.GetByIdAsync(requestId))
            .ReturnsAsync(existingRequest);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.RequestEditHistoryRepository.CreateAsync(
            It.Is<RequestEditHistory>(h =>
                h.RequestId == requestId &&
                h.EditedByUserId == userId &&
                h.OldFormData == "{\"start\":\"2025-01-15\",\"end\":\"2025-01-20\"}" &&
                h.NewFormData == "{\"start\":\"2025-01-16\",\"end\":\"2025-01-21\"}" &&
                h.ChangeReason == "Zmiana terminu o 1 dzień")), Times.Once);
    }

    [Fact]
    public async Task Handle_ApprovedApprovers_ReceiveNotifications()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var approverId1 = Guid.NewGuid();
        var approverId2 = Guid.NewGuid();
        var approverId3 = Guid.NewGuid();

        var existingRequest = new Request
        {
            Id = requestId,
            RequestNumber = "REQ-004",
            SubmittedById = userId,
            Status = RequestStatus.InReview,
            FormData = "{\"old\":\"data\"}",
            SubmittedAt = DateTime.UtcNow,
            ApprovalSteps = new List<RequestApprovalStep>
            {
                new RequestApprovalStep
                {
                    Id = Guid.NewGuid(),
                    ApproverId = approverId1,
                    Status = ApprovalStepStatus.Approved // Should be notified
                },
                new RequestApprovalStep
                {
                    Id = Guid.NewGuid(),
                    ApproverId = approverId2,
                    Status = ApprovalStepStatus.InReview // Should NOT be notified
                },
                new RequestApprovalStep
                {
                    Id = Guid.NewGuid(),
                    ApproverId = approverId3,
                    Status = ApprovalStepStatus.Rejected // Should be notified
                }
            }
        };

        var command = new EditRequestCommand
        {
            RequestId = requestId,
            EditedByUserId = userId,
            NewFormData = "{\"new\":\"data\"}",
            ChangeReason = "Korekta"
        };

        _validatorServiceMock.Setup(x => x.ValidateAsync(command))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.RequestRepository.GetByIdAsync(requestId))
            .ReturnsAsync(existingRequest);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _notificationServiceMock.Verify(x => x.CreateNotificationAsync(
            approverId1,
            NotificationType.RequestEdited,
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once, "approved approver should be notified");

        _notificationServiceMock.Verify(x => x.CreateNotificationAsync(
            approverId3,
            NotificationType.RequestEdited,
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once, "rejected approver should be notified");

        _notificationServiceMock.Verify(x => x.CreateNotificationAsync(
            approverId2,
            It.IsAny<NotificationType>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never, "in-review approver should NOT be notified");
    }

    [Fact]
    public async Task Handle_NoApprovers_DoesNotSendNotifications()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var existingRequest = new Request
        {
            Id = requestId,
            RequestNumber = "REQ-005",
            SubmittedById = userId,
            Status = RequestStatus.Draft,
            FormData = "{\"old\":\"data\"}",
            SubmittedAt = DateTime.UtcNow,
            ApprovalSteps = new List<RequestApprovalStep>() // Empty
        };

        var command = new EditRequestCommand
        {
            RequestId = requestId,
            EditedByUserId = userId,
            NewFormData = "{\"new\":\"data\"}",
            ChangeReason = "Test"
        };

        _validatorServiceMock.Setup(x => x.ValidateAsync(command))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.RequestRepository.GetByIdAsync(requestId))
            .ReturnsAsync(existingRequest);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _notificationServiceMock.Verify(x => x.CreateNotificationAsync(
            It.IsAny<Guid>(),
            It.IsAny<NotificationType>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never, "no approvers, no notifications");
    }

    [Fact]
    public async Task Handle_ValidRequest_CallsValidatorService()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var existingRequest = new Request
        {
            Id = requestId,
            RequestNumber = "REQ-006",
            SubmittedById = userId,
            Status = RequestStatus.Draft,
            FormData = "{\"old\":\"data\"}",
            SubmittedAt = DateTime.UtcNow,
            ApprovalSteps = new List<RequestApprovalStep>()
        };

        var command = new EditRequestCommand
        {
            RequestId = requestId,
            EditedByUserId = userId,
            NewFormData = "{\"new\":\"data\"}",
            ChangeReason = "Test"
        };

        _validatorServiceMock.Setup(x => x.ValidateAsync(command))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.RequestRepository.GetByIdAsync(requestId))
            .ReturnsAsync(existingRequest);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _validatorServiceMock.Verify(x => x.ValidateAsync(command), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidRequest_LogsInformation()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var existingRequest = new Request
        {
            Id = requestId,
            RequestNumber = "REQ-007",
            SubmittedById = userId,
            Status = RequestStatus.Draft,
            FormData = "{\"old\":\"data\"}",
            SubmittedAt = DateTime.UtcNow,
            ApprovalSteps = new List<RequestApprovalStep>()
        };

        var command = new EditRequestCommand
        {
            RequestId = requestId,
            EditedByUserId = userId,
            NewFormData = "{\"new\":\"data\"}",
            ChangeReason = "Logging test"
        };

        _validatorServiceMock.Setup(x => x.ValidateAsync(command))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.RequestRepository.GetByIdAsync(requestId))
            .ReturnsAsync(existingRequest);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request") && v.ToString()!.Contains("edited")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
