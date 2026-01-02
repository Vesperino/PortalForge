using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;
using PortalForge.Application.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Application.UseCases.Users.Commands.UpdateVacationAllowance;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Tests.Unit.Application.UseCases.Users.Commands.UpdateVacationAllowance;

public class UpdateUserVacationAllowanceCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IAuditLogService> _auditLogServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IUnifiedValidatorService> _validatorServiceMock;
    private readonly Mock<ILogger<UpdateUserVacationAllowanceCommandHandler>> _loggerMock;
    private readonly UpdateUserVacationAllowanceCommandHandler _handler;

    public UpdateUserVacationAllowanceCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _auditLogServiceMock = new Mock<IAuditLogService>();
        _notificationServiceMock = new Mock<INotificationService>();
        _validatorServiceMock = new Mock<IUnifiedValidatorService>();
        _loggerMock = new Mock<ILogger<UpdateUserVacationAllowanceCommandHandler>>();

        _handler = new UpdateUserVacationAllowanceCommandHandler(
            _unitOfWorkMock.Object,
            _auditLogServiceMock.Object,
            _notificationServiceMock.Object,
            _validatorServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserVacationAllowanceCommand
        {
            UserId = userId,
            NewAnnualDays = 30,
            Reason = "Zmiana umowy",
            RequestedByUserId = Guid.NewGuid()
        };

        _validatorServiceMock.Setup(x => x.ValidateAsync(command))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"User with ID {userId} not found");
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesVacationAllowance()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var requestedBy = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "Jan",
            LastName = "Kowalski",
            AnnualVacationDays = 26,
            VacationDaysUsed = 0,
            OnDemandVacationDaysUsed = 0,
            CircumstantialLeaveDaysUsed = 0,
            CarriedOverVacationDays = 0,
            Role = UserRole.Employee,
            CreatedAt = DateTime.UtcNow
        };

        var command = new UpdateUserVacationAllowanceCommand
        {
            UserId = userId,
            NewAnnualDays = 30,
            Reason = "Zmiana umowy - dodatkowe dni urlopu",
            RequestedByUserId = requestedBy,
            IpAddress = "192.168.1.1"
        };

        _validatorServiceMock.Setup(x => x.ValidateAsync(command))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _unitOfWorkMock.Setup(x => x.UserRepository.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);
        user.AnnualVacationDays.Should().Be(30, "vacation allowance should be updated");

        _unitOfWorkMock.Verify(x => x.UserRepository.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesAuditLog()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var requestedBy = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "Anna",
            LastName = "Nowak",
            AnnualVacationDays = 20,
            Role = UserRole.Employee,
            CreatedAt = DateTime.UtcNow
        };

        var command = new UpdateUserVacationAllowanceCommand
        {
            UserId = userId,
            NewAnnualDays = 26,
            Reason = "Wzrost stażu pracy",
            RequestedByUserId = requestedBy,
            IpAddress = "10.0.0.1"
        };

        _validatorServiceMock.Setup(x => x.ValidateAsync(command))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _auditLogServiceMock.Verify(x => x.LogActionAsync(
            "User",
            userId.ToString(),
            "VacationAllowanceUpdated",
            requestedBy,
            "20",
            "26",
            "Wzrost stażu pracy",
            "10.0.0.1"), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_SendsNotificationToUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var requestedBy = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "Piotr",
            LastName = "Zieliński",
            AnnualVacationDays = 26,
            Role = UserRole.Employee,
            CreatedAt = DateTime.UtcNow
        };

        var command = new UpdateUserVacationAllowanceCommand
        {
            UserId = userId,
            NewAnnualDays = 32,
            Reason = "Umowa o pracę na pełen etat",
            RequestedByUserId = requestedBy
        };

        _validatorServiceMock.Setup(x => x.ValidateAsync(command))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _notificationServiceMock.Verify(x => x.CreateNotificationAsync(
            userId,
            NotificationType.VacationAllowanceUpdated,
            "Zmiana limitu urlopów",
            It.Is<string>(msg => msg.Contains("26") && msg.Contains("32") && msg.Contains(command.Reason)),
            "User",
            userId.ToString(),
            "/dashboard/account"), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsValidatorService()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "Maria",
            LastName = "Kowalska",
            AnnualVacationDays = 26,
            Role = UserRole.Employee,
            CreatedAt = DateTime.UtcNow
        };

        var command = new UpdateUserVacationAllowanceCommand
        {
            UserId = userId,
            NewAnnualDays = 28,
            Reason = "Test reason",
            RequestedByUserId = Guid.NewGuid()
        };

        _validatorServiceMock.Setup(x => x.ValidateAsync(command))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _validatorServiceMock.Verify(x => x.ValidateAsync(command), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_LogsInformation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var requestedBy = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            AnnualVacationDays = 26,
            Role = UserRole.Employee,
            CreatedAt = DateTime.UtcNow
        };

        var command = new UpdateUserVacationAllowanceCommand
        {
            UserId = userId,
            NewAnnualDays = 30,
            Reason = "Test",
            RequestedByUserId = requestedBy
        };

        _validatorServiceMock.Setup(x => x.ValidateAsync(command))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Vacation allowance updated")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DecreasingAllowance_UpdatesCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "Adam",
            LastName = "Nowacki",
            AnnualVacationDays = 30,
            Role = UserRole.Employee,
            CreatedAt = DateTime.UtcNow
        };

        var command = new UpdateUserVacationAllowanceCommand
        {
            UserId = userId,
            NewAnnualDays = 20,
            Reason = "Zmiana na część etatu",
            RequestedByUserId = Guid.NewGuid()
        };

        _validatorServiceMock.Setup(x => x.ValidateAsync(command))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.AnnualVacationDays.Should().Be(20, "allowance can be decreased");

        _auditLogServiceMock.Verify(x => x.LogActionAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Guid>(),
            "30", // old value
            "20", // new value
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
    }
}
