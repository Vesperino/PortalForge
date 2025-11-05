using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;
using PortalForge.Application.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Application.UseCases.Users.Commands.TransferDepartment;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Tests.Unit.Application.UseCases.Users.Commands.TransferDepartment;

public class TransferEmployeeToDepartmentCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IAuditLogService> _auditLogServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IUnifiedValidatorService> _validatorServiceMock;
    private readonly Mock<ILogger<TransferEmployeeToDepartmentCommandHandler>> _loggerMock;
    private readonly TransferEmployeeToDepartmentCommandHandler _handler;

    public TransferEmployeeToDepartmentCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _auditLogServiceMock = new Mock<IAuditLogService>();
        _notificationServiceMock = new Mock<INotificationService>();
        _validatorServiceMock = new Mock<IUnifiedValidatorService>();
        _loggerMock = new Mock<ILogger<TransferEmployeeToDepartmentCommandHandler>>();

        _handler = new TransferEmployeeToDepartmentCommandHandler(
            _unitOfWorkMock.Object,
            _auditLogServiceMock.Object,
            _notificationServiceMock.Object,
            _validatorServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_TransfersUserSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var oldDepartmentId = Guid.NewGuid();
        var newDepartmentId = Guid.NewGuid();
        var transferredByUserId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            DepartmentId = oldDepartmentId,
            Role = UserRole.Employee
        };

        var newDepartment = new Department
        {
            Id = newDepartmentId,
            Name = "New Department",
            IsActive = true
        };

        var command = new TransferEmployeeToDepartmentCommand
        {
            UserId = userId,
            NewDepartmentId = newDepartmentId,
            NewSupervisorId = null,
            TransferredByUserId = transferredByUserId,
            Reason = "Organizational restructuring"
        };

        _validatorServiceMock
            .Setup(x => x.ValidateAsync(command))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _unitOfWorkMock.Setup(x => x.DepartmentRepository.GetByIdAsync(newDepartmentId))
            .ReturnsAsync(newDepartment);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);

        user.DepartmentId.Should().Be(newDepartmentId);

        _unitOfWorkMock.Verify(x => x.UserRepository.UpdateAsync(user), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);

        _auditLogServiceMock.Verify(x => x.LogActionAsync(
            "User",
            userId.ToString(),
            "DepartmentTransfer",
            transferredByUserId,
            It.IsAny<string>(),
            It.IsAny<string>(),
            command.Reason,
            null), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var command = new TransferEmployeeToDepartmentCommand
        {
            UserId = Guid.NewGuid(),
            NewDepartmentId = Guid.NewGuid(),
            TransferredByUserId = Guid.NewGuid()
        };

        _validatorServiceMock
            .Setup(x => x.ValidateAsync(command))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(command.UserId))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"User with ID {command.UserId} not found");

        _unitOfWorkMock.Verify(x => x.UserRepository.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DepartmentNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newDepartmentId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Email = "john.doe@test.com",
            DepartmentId = Guid.NewGuid(),
            Role = UserRole.Employee
        };

        var command = new TransferEmployeeToDepartmentCommand
        {
            UserId = userId,
            NewDepartmentId = newDepartmentId,
            TransferredByUserId = Guid.NewGuid()
        };

        _validatorServiceMock
            .Setup(x => x.ValidateAsync(command))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _unitOfWorkMock.Setup(x => x.DepartmentRepository.GetByIdAsync(newDepartmentId))
            .ReturnsAsync((Department?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Department with ID {newDepartmentId} not found");

        _unitOfWorkMock.Verify(x => x.UserRepository.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NewSupervisorNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newDepartmentId = Guid.NewGuid();
        var newSupervisorId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Email = "john.doe@test.com",
            DepartmentId = Guid.NewGuid(),
            Role = UserRole.Employee
        };

        var newDepartment = new Department
        {
            Id = newDepartmentId,
            Name = "New Department",
            IsActive = true
        };

        var command = new TransferEmployeeToDepartmentCommand
        {
            UserId = userId,
            NewDepartmentId = newDepartmentId,
            NewSupervisorId = newSupervisorId,
            TransferredByUserId = Guid.NewGuid()
        };

        _validatorServiceMock
            .Setup(x => x.ValidateAsync(command))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _unitOfWorkMock.Setup(x => x.DepartmentRepository.GetByIdAsync(newDepartmentId))
            .ReturnsAsync(newDepartment);

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(newSupervisorId))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"New supervisor with ID {newSupervisorId} not found");

        _unitOfWorkMock.Verify(x => x.UserRepository.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    // NOTE: This test has been removed because pending requests are no longer reassigned during department transfers.
    // Approvers are determined by department structure (HeadOfDepartmentId, DirectorId) dynamically.

    [Fact]
    public async Task Handle_SendsNotificationToOldDepartmentHead()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var oldDepartmentId = Guid.NewGuid();
        var oldDepartmentHeadId = Guid.NewGuid();
        var newDepartmentId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            DepartmentId = oldDepartmentId,
            Role = UserRole.Employee
        };

        var oldDepartment = new Department
        {
            Id = oldDepartmentId,
            Name = "Old Department",
            HeadOfDepartmentId = oldDepartmentHeadId
        };

        var command = new TransferEmployeeToDepartmentCommand
        {
            UserId = userId,
            NewDepartmentId = newDepartmentId,
            TransferredByUserId = Guid.NewGuid()
        };

        _validatorServiceMock
            .Setup(x => x.ValidateAsync(command))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _unitOfWorkMock.Setup(x => x.DepartmentRepository.GetByIdAsync(oldDepartmentId))
            .ReturnsAsync(oldDepartment);

        _unitOfWorkMock.Setup(x => x.DepartmentRepository.GetByIdAsync(newDepartmentId))
            .ReturnsAsync(new Department { Id = newDepartmentId, Name = "New Dept" });

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _notificationServiceMock.Verify(x => x.CreateNotificationAsync(
            oldDepartmentHeadId,
            NotificationType.System,
            "Pracownik przeniesiony",
            It.Is<string>(s => s.Contains("John Doe") && s.Contains("przeniesiony do innego działu")),
            null,
            null,
            null), Times.Once);
    }

    [Fact]
    public async Task Handle_SendsNotificationToNewDepartmentHead()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newDepartmentId = Guid.NewGuid();
        var newDepartmentHeadId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            DepartmentId = Guid.NewGuid(),
            Role = UserRole.Employee
        };

        var newDepartment = new Department
        {
            Id = newDepartmentId,
            Name = "New Department",
            HeadOfDepartmentId = newDepartmentHeadId
        };

        var command = new TransferEmployeeToDepartmentCommand
        {
            UserId = userId,
            NewDepartmentId = newDepartmentId,
            TransferredByUserId = Guid.NewGuid()
        };

        _validatorServiceMock
            .Setup(x => x.ValidateAsync(command))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _unitOfWorkMock.Setup(x => x.DepartmentRepository.GetByIdAsync(newDepartmentId))
            .ReturnsAsync(newDepartment);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _notificationServiceMock.Verify(x => x.CreateNotificationAsync(
            newDepartmentHeadId,
            NotificationType.System,
            "Nowy pracownik",
            It.Is<string>(s => s.Contains("John Doe") && s.Contains("przeniesiony do Twojego działu")),
            "User",
            userId.ToString(),
            $"/dashboard/users/{userId}"), Times.Once);
    }

    // NOTE: This test has been removed because pending requests are no longer reassigned during department transfers.
    // The handler does not interact with RequestRepository at all.

    [Fact]
    public async Task Handle_CallsValidatorService()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newDepartmentId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Email = "john.doe@test.com",
            DepartmentId = Guid.NewGuid(),
            Role = UserRole.Employee
        };

        var command = new TransferEmployeeToDepartmentCommand
        {
            UserId = userId,
            NewDepartmentId = newDepartmentId,
            TransferredByUserId = Guid.NewGuid()
        };

        SetupValidCommand(command, user, newDepartmentId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _validatorServiceMock.Verify(x => x.ValidateAsync(command), Times.Once);
    }

    [Fact]
    public async Task Handle_CreatesAuditLog()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var oldDepartmentId = Guid.NewGuid();
        var newDepartmentId = Guid.NewGuid();
        var transferredByUserId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Email = "john.doe@test.com",
            DepartmentId = oldDepartmentId,
            Role = UserRole.Employee
        };

        var command = new TransferEmployeeToDepartmentCommand
        {
            UserId = userId,
            NewDepartmentId = newDepartmentId,
            TransferredByUserId = transferredByUserId,
            Reason = "Organizational restructuring"
        };

        SetupValidCommand(command, user, newDepartmentId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _auditLogServiceMock.Verify(x => x.LogActionAsync(
            "User",
            userId.ToString(),
            "DepartmentTransfer",
            transferredByUserId,
            It.Is<string>(s => s.Contains(oldDepartmentId.ToString())),
            It.Is<string>(s => s.Contains(newDepartmentId.ToString())),
            "Organizational restructuring",
            null), Times.Once);
    }

    [Fact]
    public async Task Handle_LogsInformation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var oldDepartmentId = Guid.NewGuid();
        var newDepartmentId = Guid.NewGuid();
        var transferredByUserId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Email = "john.doe@test.com",
            DepartmentId = oldDepartmentId,
            Role = UserRole.Employee
        };

        var command = new TransferEmployeeToDepartmentCommand
        {
            UserId = userId,
            NewDepartmentId = newDepartmentId,
            TransferredByUserId = transferredByUserId
        };

        SetupValidCommand(command, user, newDepartmentId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("transferred")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    private void SetupValidCommand(TransferEmployeeToDepartmentCommand command, User user, Guid newDepartmentId)
    {
        _validatorServiceMock
            .Setup(x => x.ValidateAsync(command))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(command.UserId))
            .ReturnsAsync(user);

        _unitOfWorkMock.Setup(x => x.DepartmentRepository.GetByIdAsync(newDepartmentId))
            .ReturnsAsync(new Department { Id = newDepartmentId, Name = "New Dept" });

        if (command.NewSupervisorId.HasValue)
        {
            _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(command.NewSupervisorId.Value))
                .ReturnsAsync(new User { Id = command.NewSupervisorId.Value, Role = UserRole.Manager });
        }
    }
}
