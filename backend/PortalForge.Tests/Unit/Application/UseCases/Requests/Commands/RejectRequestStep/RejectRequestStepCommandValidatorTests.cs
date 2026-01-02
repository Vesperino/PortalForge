using FluentAssertions;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.Requests.Commands.RejectRequestStep;
using PortalForge.Application.UseCases.Requests.Commands.RejectRequestStep.Validation;
using PortalForge.Domain.Entities;
using Xunit;

namespace PortalForge.Tests.Unit.Application.UseCases.Requests.Commands.RejectRequestStep;

public class RejectRequestStepCommandValidatorTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RejectRequestStepCommandValidator _validator;

    public RejectRequestStepCommandValidatorTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _validator = new RejectRequestStepCommandValidator(_unitOfWorkMock.Object);
        SetupDefaultMocks();
    }

    private void SetupDefaultMocks()
    {
        // Setup default mocks for empty/invalid values that trigger async validation
        _unitOfWorkMock.Setup(x => x.RequestRepository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Request?)null);

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
    }

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var stepId = Guid.NewGuid();
        var approverId = Guid.NewGuid();

        var command = new RejectRequestStepCommand
        {
            RequestId = requestId,
            StepId = stepId,
            ApproverId = approverId,
            Reason = "Not approved due to budget constraints"
        };

        // Override default mocks for valid case
        _unitOfWorkMock.Setup(x => x.RequestRepository.GetByIdAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Request { Id = requestId });

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(approverId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = approverId });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_EmptyRequestId_FailsValidation()
    {
        // Arrange
        var command = new RejectRequestStepCommand
        {
            RequestId = Guid.Empty,
            StepId = Guid.NewGuid(),
            ApproverId = Guid.NewGuid(),
            Reason = "Valid reason"
        };

        _unitOfWorkMock.Setup(x => x.RequestRepository.GetByIdAsync(Guid.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Request?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "RequestId");
    }

    [Fact]
    public async Task Validate_EmptyStepId_FailsValidation()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var command = new RejectRequestStepCommand
        {
            RequestId = requestId,
            StepId = Guid.Empty,
            ApproverId = Guid.NewGuid(),
            Reason = "Valid reason"
        };

        _unitOfWorkMock.Setup(x => x.RequestRepository.GetByIdAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Request { Id = requestId });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "StepId" &&
            e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public async Task Validate_EmptyApproverId_FailsValidation()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var command = new RejectRequestStepCommand
        {
            RequestId = requestId,
            StepId = Guid.NewGuid(),
            ApproverId = Guid.Empty,
            Reason = "Valid reason"
        };

        _unitOfWorkMock.Setup(x => x.RequestRepository.GetByIdAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Request { Id = requestId });

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(Guid.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ApproverId");
    }

    [Fact]
    public async Task Validate_EmptyReason_FailsValidation()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var approverId = Guid.NewGuid();

        var command = new RejectRequestStepCommand
        {
            RequestId = requestId,
            StepId = Guid.NewGuid(),
            ApproverId = approverId,
            Reason = string.Empty
        };

        _unitOfWorkMock.Setup(x => x.RequestRepository.GetByIdAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Request { Id = requestId });

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(approverId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = approverId });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Reason" &&
            e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public async Task Validate_ReasonTooShort_FailsValidation()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var approverId = Guid.NewGuid();

        var command = new RejectRequestStepCommand
        {
            RequestId = requestId,
            StepId = Guid.NewGuid(),
            ApproverId = approverId,
            Reason = "Short" // Less than 10 characters
        };

        _unitOfWorkMock.Setup(x => x.RequestRepository.GetByIdAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Request { Id = requestId });

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(approverId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = approverId });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Reason" &&
            e.ErrorMessage.Contains("at least 10 characters"));
    }

    [Fact]
    public async Task Validate_ReasonTooLong_FailsValidation()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var approverId = Guid.NewGuid();

        var command = new RejectRequestStepCommand
        {
            RequestId = requestId,
            StepId = Guid.NewGuid(),
            ApproverId = approverId,
            Reason = new string('a', 1001) // 1001 characters
        };

        _unitOfWorkMock.Setup(x => x.RequestRepository.GetByIdAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Request { Id = requestId });

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(approverId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = approverId });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Reason" &&
            e.ErrorMessage.Contains("1000 characters"));
    }

    [Fact]
    public async Task Validate_RequestDoesNotExist_FailsValidation()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var approverId = Guid.NewGuid();

        var command = new RejectRequestStepCommand
        {
            RequestId = requestId,
            StepId = Guid.NewGuid(),
            ApproverId = approverId,
            Reason = "Valid rejection reason"
        };

        _unitOfWorkMock.Setup(x => x.RequestRepository.GetByIdAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Request?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "RequestId" &&
            e.ErrorMessage.Contains("does not exist"));
    }

    [Fact]
    public async Task Validate_ApproverDoesNotExist_FailsValidation()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var approverId = Guid.NewGuid();

        var command = new RejectRequestStepCommand
        {
            RequestId = requestId,
            StepId = Guid.NewGuid(),
            ApproverId = approverId,
            Reason = "Valid rejection reason"
        };

        _unitOfWorkMock.Setup(x => x.RequestRepository.GetByIdAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Request { Id = requestId });

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(approverId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "ApproverId" &&
            e.ErrorMessage.Contains("does not exist"));
    }
}
